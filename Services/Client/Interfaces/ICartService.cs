using StoreBlazor.DTO.Client;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface ICartService
    {
        event Action? OnChange;

        Task InitializeAsync();         

        // Cart methods
        List<CartItemDTO> GetCart();
        Task AddToCartAsync(CartItemDTO item);
        Task RemoveItemAsync(int productId);
        Task ClearCartAsync();

        int GetTotalQuantity();
        decimal GetSubTotalAmount();

        Task UpdateQuantityAsync(int productId, int quantity);

        // Promotion methods
        Promotion? GetAppliedPromotion();
        decimal GetPromotionAmount(decimal subTotalAmount);
        decimal GetFinalAmount();
        Task ApplyPromotionAsync(Promotion promo);
        Task RemovePromotionAsync();

    }
}
