using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IOrderManagerService
    {
        Task<PageResult<OrderTableDto>> GetAllOrdersForTableAsync(int page);
        Task<ServiceResult> ApproveAsync(int orderId);
        Task<ServiceResult> CancelAsync(int orderId);
        Task<OrderDetailDto?> GetOrderDetailAsync(int orderId);
        Task<PageResult<OrderTableDto>> FilterAsync(string keyword, int status, decimal? priceFrom, decimal? priceTo, int page);

        // Cho client
        Task<PageResult<OrderTableDto>> GetOrdersByCustomerAsync(int userId, int page);
        Task<PageResult<OrderTableDto>> FilterByCustomerAsync(int userId, string keyword, int status, int page);
    }

}

