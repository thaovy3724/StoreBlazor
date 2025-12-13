using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.DTO.Payment;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;
using StoreBlazor.Services.Payment;
using System.Net;

namespace StoreBlazor.Services.Client.Implementations
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IVNPayService _vnpayService;
        private readonly IMoMoService _momoService;
        public OrderService(ApplicationDbContext dbContext,
                            IVNPayService vnpayService,
                            IMoMoService momoService) : base(dbContext)
        {
            _vnpayService = vnpayService;
            _momoService = momoService;
        }

        public async Task<ServiceResult> CreateOrderWithPaymentAsync(
                                        OrderCreateClientDTO orderDTO)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Order trong database
                var order = new Order
                {
                    CustomerId = orderDTO.CustomerId,
                    UserId = orderDTO.UserId,
                    PromoId = orderDTO.PromoId,
                    Address = orderDTO.Address,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending, // Chờ thanh toán
                    TotalAmount = orderDTO.TotalAmount,
                    DiscountAmount = orderDTO.DiscountAmount
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                // 2. Tạo OrderItems
                foreach (var item in orderDTO.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Subtotal = item.SubTotal
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
                if (orderDTO.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions.FindAsync(orderDTO.PromoId.Value);
                    if (promo != null) promo.UsedCount++;
                }

                await _dbContext.SaveChangesAsync(); // lưu vào db 
                await transaction.CommitAsync();

                // 3. Xử lý thanh toán theo phương thức
                string paymentUrl = string.Empty;

                if (orderDTO.PaymentMethod == PaymentMethod.BankTransfer)
                {
                    // VNPay
                    var vnpayRequest = new VNPayRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = orderDTO.TotalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}"
                    };
                    paymentUrl = _vnpayService.CreatePaymentUrl(vnpayRequest, true);
                }
                else if (orderDTO.PaymentMethod == PaymentMethod.EWallet)
                {
                    // MoMo
                    var momoRequest = new MoMoRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = orderDTO.TotalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}"
                    };
                    var momoResponse = await _momoService.CreatePaymentAsync(momoRequest, true);

                    if (!momoResponse.Success)
                    {
                        // update order status to cancelled
                        await CancelOrderAsync(order.OrderId);
                        return new ServiceResult { Type = "error", Message = momoResponse.Message };
                    }
                    else paymentUrl = momoResponse.PayUrl;
                }
                else
                {
                    // Thanh toán trực tiếp (Cash/Card)
                    await UpdateOrderStatusAfterPaymentAsync(order.OrderId, orderDTO.PaymentMethod);
                    paymentUrl = $"/client/payment-success/{order.OrderId}";
                }


                return new ServiceResult
                {
                    Type = "success",
                    Message = paymentUrl  // Trả về URL để redirect
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
