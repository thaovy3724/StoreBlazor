using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class OrderManagerService: BasePaginationService, IOrderManagerService
    {
        public OrderManagerService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }


        public async Task<PageResult<OrderTableDto>> GetAllOrdersForTableAsync(int page)
        {
            var query = _dbContext.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderTableDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                });

            return await GetPagedAsync(query, page);

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
        public Task<PageResult<OrderTableDto>> FilterAsync(string keyword, int status, decimal? priceFrom, decimal? priceTo, int page)
        {
            var query = _dbContext.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                if (int.TryParse(keyword, out int orderId))
                {
                    query = query.Where(o => o.OrderId == orderId);
                }
                else
                {
                    query = query.Where(o => o.Customer != null && o.Customer.Name.Contains(keyword)
                                          || o.Customer == null && "Khách lẻ".Contains(keyword));
                }
            }

            if (status != -1)
            {
                query = query.Where(o => (int)o.Status == status);
            }

            if (priceFrom.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= priceTo.Value);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var dtoQuery = query.Select(o => new OrderTableDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            });

            return GetPagedAsync<OrderTableDto>(dtoQuery, page);
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

        // Lấy đơn hàng của 1 user cụ thể
        public async Task<PageResult<OrderTableDto>> GetOrdersByCustomerAsync(int userId, int page)
        {
            Console.WriteLine($"[SERVICE] GetOrdersByCustomerAsync - userId: {userId}, page: {page}");

            var query = _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Where(o => o.UserId == userId) 
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderTableDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                });

            var result = await GetPagedAsync(query, page);
            Console.WriteLine($"[SERVICE] Result - Items: {result.Items.Count}, TotalPages: {result.TotalPages}");

            return result;
        }

        // Lọc đơn hàng của 1 user cụ thể
        public async Task<PageResult<OrderTableDto>> FilterByCustomerAsync(int userId, string keyword, int status, int page)
        {
            Console.WriteLine($"[SERVICE] FilterByCustomerAsync - userId: {userId}, keyword: '{keyword}', status: {status}");

            var query = _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Where(o => o.UserId == userId) 
                .AsQueryable();

            // Lọc theo mã đơn hàng
            if (!string.IsNullOrEmpty(keyword) && int.TryParse(keyword, out int orderId))
            {
                query = query.Where(o => o.OrderId == orderId);
            }

            // Lọc theo trạng thái
            if (status != -1)
            {
                query = query.Where(o => (int)o.Status == status);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var dtoQuery = query.Select(o => new OrderTableDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            });

            return await GetPagedAsync<OrderTableDto>(dtoQuery, page);
        }
    }
}
