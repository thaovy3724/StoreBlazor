using Microsoft.JSInterop;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;
using System.Text.Json;

namespace StoreBlazor.Services.Client.Implementations
{
    public class CartService : ICartService
    {
        private readonly IJSRuntime JsHelper;
        private readonly List<CartItemDTO> Cart = new();
        private Promotion? _appliedPromotion;

        private const string StorageKey = "cart_items";
        private const string PromotionStorageKey = "cart_promotion";

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
            }

            var promo = await JsHelper.InvokeAsync<Promotion?>(
                "CartStorage.loadPromotion",
                PromotionStorageKey
            );

            if (promo != null)
            {
                _appliedPromotion = promo;
            }
            OnChange?.Invoke();   // Báo cho UI biết giỏ đã có dữ liệu
        }

        public List<CartItemDTO> GetCart() => Cart;

        private async Task PersistAsync()
        {
            await JsHelper.InvokeVoidAsync("CartStorage.save", StorageKey, Cart);
            if (_appliedPromotion != null)
            {
                await JsHelper.InvokeVoidAsync("CartStorage.savePromotion",
                    PromotionStorageKey,
                    _appliedPromotion);
            }
        }

        public async Task AddToCartAsync(CartItemDTO item)
        {
            var existing = Cart.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existing != null)
                existing.SelectedQuantity += item.SelectedQuantity;
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
            _appliedPromotion = null;

            await JsHelper.InvokeVoidAsync("CartStorage.clear", StorageKey);
            await JsHelper.InvokeVoidAsync("CartStorage.clearPromotion", PromotionStorageKey);

            OnChange?.Invoke();
        }

        public int GetTotalQuantity()
        {
            return Cart.Sum(x => x.SelectedQuantity);
        }

        public decimal GetSubTotalAmount()
        {
            return Cart.Sum(x => x.SelectedQuantity*x.ProductPrice);
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            var existing = Cart.FirstOrDefault(x => x.ProductId == productId);

            if (existing != null)
            {
                existing.SelectedQuantity = quantity;

                await PersistAsync();
                OnChange?.Invoke();
            }
        }

        // ==== PROMOTION ====
        public Promotion? GetAppliedPromotion()
        {
            return _appliedPromotion;
        }

        public decimal GetPromotionAmount(decimal subTotalAmount)
        {
            if (_appliedPromotion == null)
                return 0;

            // Nếu áp dụng min order
            if (subTotalAmount < _appliedPromotion.MinOrderAmount)
                return 0;

            if (_appliedPromotion.DiscountType == DiscountType.Percent)
            {
                var discount = subTotalAmount * (_appliedPromotion.DiscountValue / 100m);
                return discount;
            }
            else if (_appliedPromotion.DiscountType == DiscountType.Fixed)
            {
                return _appliedPromotion.DiscountValue;
            }

            return 0;
        }

        public decimal GetFinalAmount()
        {
            var total = GetSubTotalAmount();
            var discount = GetPromotionAmount(total);

            var final = total - discount;
            return final < 0 ? 0 : final;
        }

        // ÁP DỤNG MÃ (LƯU SESSION)
        public async Task ApplyPromotionAsync(Promotion promo)
        {
            _appliedPromotion = promo;

            await JsHelper.InvokeVoidAsync(
                "CartStorage.savePromotion",
                PromotionStorageKey,
                promo
            );

            OnChange?.Invoke();
        }

        // XOÁ MÃ (XOÁ SESSION)
        public async Task RemovePromotionAsync()
        {
            _appliedPromotion = null;

            await JsHelper.InvokeVoidAsync(
                "CartStorage.clearPromotion",
                PromotionStorageKey
            );

            OnChange?.Invoke();
        }
    }
}
