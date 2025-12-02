using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using StoreBlazor.Services.Interfaces;

namespace StoreBlazor.Services.Implementations
{
    public class OrderManagerService:IOrderManagerService
    {
        private readonly ApplicationDbContext _dbContext;
        public OrderManagerService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<List<OrderTableDto>> GetAllOrdersForTableAsync()
        {
            var data = await _dbContext.Orders
                .Include(o => o.Customer)     
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderTableDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            return data;
        }
        public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Promotion)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            var dto = new OrderDetailDto
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer?.Name ?? "",
                UserName = order.User?.FullName ?? "",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                PromoCode = order.Promotion?.PromoCode ?? "Không có",
                PaymentMethod = order.Payment.PaymentMethod,
                PaymentAmount = order.Payment?.Amount ?? 0,
                PaymentDate = order.Payment?.PaymentDate,
                Items = order.OrderItems?.Select(oi => new OrderDetailDto.OrderItemDto
                {
                    ProductName = oi.Product?.ProductName ?? "",
                    ProductImage = oi.Product?.ProductImage ?? "",
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList() ?? new List<OrderDetailDto.OrderItemDto>()
            };

            return dto;
        }



        public async Task<ServiceResult> ApproveAsync(int orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
                return new ServiceResult { Type = "error", Message = "Đơn hàng không tồn tại" };

            order.Status = OrderStatus.Paid;
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Đã duyệt đơn hàng thành công!"
            };
        }

        public async Task<ServiceResult> CancelAsync(int orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
                return new ServiceResult { Type = "error", Message = "Đơn hàng không tồn tại" };

            order.Status = OrderStatus.Cancelled;
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Đã hủy đơn hàng thành công!"
            };
        }
    }
}
