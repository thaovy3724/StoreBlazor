using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using System.Drawing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Interfaces
{
    public interface IOrderManagerService
    {
        Task<List<OrderTableDto>> GetAllOrdersForTableAsync();
        Task<ServiceResult> ApproveAsync(int orderId);
        Task<ServiceResult> CancelAsync(int orderId);
        Task<OrderDetailDto?> GetOrderDetailAsync(int orderId);

    }
}
