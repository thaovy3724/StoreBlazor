using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult> CreateOrderWithPaymentAsync(OrderCreateClientDTO orderDto);

        Task<ServiceResult> UpdateOrderStatusAfterPaymentAsync(int orderId, PaymentMethod paymentMethod);
        Task<ServiceResult> CancelOrderAsync(int orderId);
    }
}
