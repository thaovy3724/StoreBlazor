using Microsoft.JSInterop;
using StoreBlazor.DTO.Client;
using StoreBlazor.Services.Client.Interfaces;
using System.Text.Json;

namespace StoreBlazor.Services.Client.Implementations
{
    public class CartService : ICartService
    {
        private readonly IJSRuntime JsHelper;
        private readonly List<CartItemDTO> Cart = new();
        private const string StorageKey = "cart_items";

        public event Action? OnChange;

        public CartService(IJSRuntime js)
        {
            JsHelper = js;
        }

        public async Task InitializeAsync()
        {
            // Gọi CartStorage.load bên JS, trả về List<CartItemDTO> hoặc null
            var items = await JsHelper.InvokeAsync<List<CartItemDTO>?>(
                "CartStorage.load",
                StorageKey
            );

            if (items is not null)
            {
                Cart.Clear();
                Cart.AddRange(items);
                OnChange?.Invoke();   // Báo cho UI biết giỏ đã có dữ liệu
            }
        }

        public List<CartItemDTO> GetCart() => Cart;

        private async Task PersistAsync()
        {
            await JsHelper.InvokeVoidAsync("CartStorage.save", StorageKey, Cart);
        }

        public async Task AddToCartAsync(CartItemDTO item)
        {
            var existing = Cart.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                Cart.Add(item);

            await PersistAsync();
            OnChange?.Invoke();
        }

        public async Task RemoveItemAsync(int productId)
        {
            var item = Cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
                Cart.Remove(item);

            await PersistAsync();
            OnChange?.Invoke();
        }

        public async Task ClearCartAsync()
        {
            Cart.Clear();
            await JsHelper.InvokeVoidAsync("CartStorage.clear", StorageKey);
            OnChange?.Invoke();
        }
    }
}
