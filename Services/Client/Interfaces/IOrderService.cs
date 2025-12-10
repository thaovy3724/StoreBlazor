using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult> CreateOrderWithPaymentAsync(OrderCreateClientDTO orderDTO);
    }
}
