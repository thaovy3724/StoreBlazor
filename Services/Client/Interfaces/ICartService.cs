using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface ICartService
    {
        event Action? OnChange;

        Task InitializeAsync();                // load từ localStorage
        Task AddToCartAsync(CartItemDTO item);
        Task RemoveItemAsync(int productId);
        Task ClearCartAsync();

        List<CartItemDTO> GetCart();
    }
}
