using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Payment;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;
using StoreBlazor.Services.Payment;

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

            var kw = keyword.Trim();

            return await _dbContext.Customers
                .Where(c => c.Phone != null && EF.Functions.Like(c.Phone, $"%{kw}%"))
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
        public async Task<ServiceResult> CreateOrderWithPaymentAsync(
                                        OrderCreateDto orderDto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Order trong database
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    UserId = orderDto.UserId,
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
                    // Nếu thanh toán thất bại thì cộng lại tồn kho sau
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

                // Cập nhật UsedCount của Promotion
                if (orderDto.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions.FindAsync(orderDto.PromoId.Value);
                    if (promo != null) promo.UsedCount++;
                }

                await _dbContext.SaveChangesAsync(); // lưu vào db 


                // 3. Xử lý thanh toán theo phương thức
                string paymentUrl = string.Empty;

                if (orderDto.PaymentMethod == PaymentMethod.BankTransfer)
                {
                    // VNPay
                    var vnpayRequest = new VNPayRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = orderDto.FinalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}"
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
                        // update order status to cancelled
                        return new ServiceResult { Type = "error", Message = momoResponse.Message };
                    }else  paymentUrl = momoResponse.PayUrl;
                }
                else
                {
                    // Thanh toán trực tiếp (Cash/Card)
                    UpdateOrderStatusAfterPaymentAsync(order.OrderId, orderDto.PaymentMethod);
                }

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
        public async Task<ServiceResult> UpdateOrderStatusAfterPaymentAsync(
                                        int orderId,
                                        PaymentMethod paymentMethod)
        {
            try
            {
                var order = await _dbContext.Orders.FindAsync(orderId);

                if (order == null)
                {
                    return new ServiceResult { Type = "error", Message = "Đơn hàng không tồn tại" };
                }

                // Cập nhật trạng thái đơn hàng
                order.Status = OrderStatus.Paid;

                // Tạo Payment record
                var payment = new StoreBlazor.Models.Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount - order.DiscountAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = paymentMethod
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = "Cập nhật trạng thái thành công"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Lỗi cập nhật: " + ex.Message
                };
            }
        }

        public async Task<ServiceResult> CancelOrderAsync(int orderId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy order
                var order = await _dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                    return new ServiceResult { Type = "error", Message = "Không tìm thấy đơn hàng." };

                // Không cho phép hủy nếu đã thanh toán
                if (order.Status != OrderStatus.Pending)
                    return new ServiceResult { Type = "error", Message = "Đơn hàng không thể hủy." };

                // 2. Khôi phục tồn kho của từng sản phẩm
                foreach (var item in order.OrderItems)
                {
                    var inventory = await _dbContext.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity += item.Quantity;
                        inventory.UpdatedAt = DateTime.Now;
                    }
                }

                // 3. Tăng UsedCount của Promotion nếu có
                if (order.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions
                        .FirstOrDefaultAsync(p => p.PromoId == order.PromoId.Value);

                    if (promo != null && promo.UsedCount > 0)
                        promo.UsedCount++;
                }

                // 4. Cập nhật trạng thái order
                order.Status = OrderStatus.Cancelled;
                order.OrderDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = $"Đơn hàng #{orderId} đã được hủy thành công."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Lỗi khi hủy đơn hàng: " + ex.Message
                };
            }
        }
    }
    }