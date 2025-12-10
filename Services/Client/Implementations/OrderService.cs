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

        public async Task<ServiceResult> CreateOrderWithPaymentAsync(OrderCreateClientDTO orderDTO)
        {
            

            using var transaction = await _dbContext.Database.BeginTransactionAsync(); // start transaction

            try
            {
               
                // 1. Tạo Order trong database
                var order = new Order
                {
                    CustomerId = orderDTO.CustomerId,
                    UserId = orderDTO.UserId,
                    PromoId = orderDTO.PromoId,
                    OrderDate = DateTime.Now,
                    TotalAmount = orderDTO.TotalAmount,
                    DiscountAmount = orderDTO.DiscountAmount,
                    Status = OrderStatus.Pending // Chờ thanh toán
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

                await _dbContext.SaveChangesAsync();

                // 3. Xử lý thanh toán theo phương thức
                string paymentUrl = string.Empty;

                if (orderDTO.PaymentMethod == PaymentMethod.BankTransfer)
                {
                    // VNPay
                    var vnpayRequest = new VNPayRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = order.TotalAmount,
                        OrderInfo = $"Thanh toan don hang #{order.OrderId}"
                    };
                    paymentUrl = _vnpayService.CreatePaymentUrl(vnpayRequest);
                }
                else if (orderDTO.PaymentMethod == PaymentMethod.EWallet)
                {
                    // MoMo
                    var momoRequest = new MoMoRequestDto
                    {
                        OrderId = order.OrderId.ToString(),
                        Amount = order.TotalAmount,
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
                else paymentUrl = "clien/payment_success"; // Thanh toán trực tiếp

                // Cập nhật UsedCount của Promotion
                // Nếu thanh toán thất bại thì cập nhật lại tồn kho 
                if (order.PromoId.HasValue)
                {
                    var promo = await _dbContext.Promotions.FindAsync(order.PromoId.Value);
                    if (promo != null) promo.UsedCount++;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync(); // end transaction

                return new ServiceResult
                {
                    Type = "success",
                    Message = paymentUrl
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
