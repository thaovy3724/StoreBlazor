using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using System.Drawing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IOrderManagerService
    {
        Task<PageResult<OrderTableDto>> GetAllOrdersForTableAsync(int page);
        Task<ServiceResult> ApproveAsync(int orderId);
        Task<ServiceResult> CancelAsync(int orderId);
        Task<OrderDetailDto?> GetOrderDetailAsync(int orderId);

    }
}
