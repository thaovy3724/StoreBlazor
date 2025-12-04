using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Payment;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;
using StoreBlazor.Services.Payment.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class OrderStaffService : BasePaginationService, IOrderStaffService
    {
        private readonly IVNPayService _vnpayService;
        private readonly IMoMoService _momoService;

        public OrderStaffService(
         ApplicationDbContext dbContext,
         IVNPayService vnpayService,
         IMoMoService momoService) : base(dbContext)
        {
            _vnpayService = vnpayService;
            _momoService = momoService;
        }

        // ===== SẢN PHẨM =====
        public async Task<PageResult<ProductCardDto>> GetProductsAsync(int page, int? categoryId = null, string? search = null)
        {
            var query = _dbContext.Products
                .Include(p => p.Inventory)
                .Where(p => p.Inventory != null && p.Inventory.Quantity > 0)
                .AsQueryable();

            // Lọc theo category
            if (categoryId.HasValue && categoryId.Value != -1)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(keyword));
            }

            var productQuery = query
                .OrderBy(p => p.ProductId)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductImage = p.ProductImage,
                    Price = p.Price,
                    Quantity = p.Inventory!.Quantity
                });

            return await GetPagedAsync(productQuery, page);
        }

        public async Task<ProductCardDto?> GetProductByBarcodeAsync(string barcode)
        {
            return await _dbContext.Products
                .Include(p => p.Inventory)
                .Where(p => p.Barcode == barcode && p.Inventory!.Quantity > 0)
                .Select(p => new ProductCardDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductImage = p.ProductImage,
                    Price = p.Price,
                    Quantity = p.Inventory!.Quantity
                })
                .FirstOrDefaultAsync();
        }

        // ===== CATEGORY =====
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        // ===== KHÁCH HÀNG =====
        public async Task<List<CustomerSuggestionDto>> SearchCustomerAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<CustomerSuggestionDto>();

            var kw = keyword.Trim().ToLower();

            return await _dbContext.Customers
                .Where(c => c.Phone != null && c.Phone.Contains(kw))
                .Take(5)
                .Select(c => new CustomerSuggestionDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone ?? "",
                    Email = c.Email,
                    Address = c.Address
                })
                .ToListAsync();
        }

        public async Task<ServiceResult> CreateCustomerAsync(Customer customer)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                return new ServiceResult { Type = "error", Message = "Vui lòng nhập tên khách hàng" };
            }

            if (string.IsNullOrWhiteSpace(customer.Phone))
            {
                return new ServiceResult { Type = "error", Message = "Vui lòng nhập số điện thoại" };
            }

            // Check trùng số điện thoại
            var exists = await _dbContext.Customers
                .AnyAsync(c => c.Phone == customer.Phone);

            if (exists)
            {
                return new ServiceResult { Type = "error", Message = "Số điện thoại đã tồn tại" };
            }

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm khách hàng thành công"
            };
        }

        // ===== KHUYẾN MÃI =====
        public async Task<List<PromotionDto>> GetActivePromotionsAsync()
        {
            var today = DateTime.Today;

            return await _dbContext.Promotions
                .Where(p => p.Status == PromotionStatus.Active
                         && p.StartDate <= today
                         && p.EndDate >= today
                         && p.UsedCount < p.UsageLimit)
                .Select(p => new PromotionDto
                {
                    PromoId = p.PromoId,
                    PromoCode = p.PromoCode,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    MinOrderAmount = p.MinOrderAmount,
                    EndDate = p.EndDate
                })
                .ToListAsync();
        }

        // ===== ĐẶT HÀNG =====
        public async Task<ServiceResult> CreateOrderAsync(OrderCreateDto orderDto, int userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Order
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    UserId = userId,
                    PromoId = orderDto.PromoId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Paid,
                    TotalAmount = orderDto.TotalAmount,
                    DiscountAmount = orderDto.DiscountAmount
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                // 2. Tạo OrderItems
                foreach (var item in orderDto.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Subtotal = item.Subtotal
                    };

                    _dbContext.OrderItems.Add(orderItem);

                    // 3. Cập nhật tồn kho
                    var inventory = await _dbContext.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory == null || inventory.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResult
                        {
                            Type = "error",
                            Message = $"Sản phẩm '{item.ProductName}' không đủ số lượng trong kho"
                        };
                    }

                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.Now;
                }

                // 4. Tạo Payment
                var payment = new StoreBlazor.Models.Payment
                {
                    OrderId = order.OrderId,
                    Amount = orderDto.TotalAmount - orderDto.DiscountAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = orderDto.PaymentMethod
                };

                _dbContext.Payments.Add(payment);

                // 5. Cập nhật UsedCount của Promotion
                if (orderDto.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions
                        .FindAsync(orderDto.PromoId.Value);

                    if (promo != null)
                    {
                        promo.UsedCount++;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = $"Đặt hàng thành công! Mã đơn: #{order.OrderId}"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Có lỗi xảy ra: " + ex.Message
                };
            }
        }
        public async Task<ServiceResult> CreateOrderWithPaymentAsync(
        OrderCreateDto orderDto,
        int userId,
        string ipAddress)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Order trong database
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    UserId = userId,
                    PromoId = orderDto.PromoId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending, // Chờ thanh toán
                    TotalAmount = orderDto.TotalAmount,
                    DiscountAmount = orderDto.DiscountAmount
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                // 2. Tạo OrderItems
                foreach (var item in orderDto.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Subtotal = item.Subtotal
                    };
                    _dbContext.OrderItems.Add(orderItem);

                    // Cập nhật tồn kho
                    var inventory = await _dbContext.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                    if (inventory == null || inventory.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResult { Type = "error", Message = $"Sản phẩm '{item.ProductName}' không đủ số lượng" };
                    }
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();

                // 3. Xử lý thanh toán theo phương thức
                string paymentUrl = string.Empty;

                if (orderDto.PaymentMethod == PaymentMethod.BankTransfer)
                {
                    // VNPay
                    var vnpayRequest = new VNPayRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = orderDto.FinalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}",
                        IpAddress = ipAddress
                    };
                    paymentUrl = _vnpayService.CreatePaymentUrl(vnpayRequest);
                }
                else if (orderDto.PaymentMethod == PaymentMethod.EWallet)
                {
                    // MoMo
                    var momoRequest = new MoMoRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = orderDto.FinalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}"
                    };
                    var momoResponse = await _momoService.CreatePaymentAsync(momoRequest);

                    if (!momoResponse.Success)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResult { Type = "error", Message = momoResponse.Message };
                    }
                    paymentUrl = momoResponse.PayUrl;
                }
                else
                {
                    // Thanh toán trực tiếp (Cash/Card)
                    order.Status = OrderStatus.Paid;
                    var payment = new StoreBlazor.Models.Payment
                    {
                        OrderId = order.OrderId,
                        Amount = orderDto.FinalAmount,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = orderDto.PaymentMethod
                    };
                    _dbContext.Payments.Add(payment);
                }

                // Cập nhật UsedCount của Promotion
                if (orderDto.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions.FindAsync(orderDto.PromoId.Value);
                    if (promo != null) promo.UsedCount++;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = !string.IsNullOrEmpty(paymentUrl)
                        ? paymentUrl  // Trả về URL để redirect
                        : $"Đặt hàng thành công! Mã đơn: #{order.OrderId}"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ServiceResult { Type = "error", Message = "Có lỗi xảy ra: " + ex.Message };
            }
        }
    }
}