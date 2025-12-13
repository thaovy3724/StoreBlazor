using Blazored.SessionStorage;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class CartService : ICartService
    {
        private readonly ISessionStorageService _session;
        private readonly List<CartItemDTO> Cart = new();
        private Promotion? _appliedPromotion;

        private const string CartKey = "cart_items";
        private const string PromotionKey = "cart_promotion";

        public event Action? OnChange;

        public CartService(ISessionStorageService session)
        {
            _session = session;
        }

        public async Task InitializeAsync()
        {
            // Gọi CartStorage.load bên JS, trả về List<CartItemDTO> hoặc null
            var items = await _session.GetItemAsync<List<CartItemDTO>>(CartKey);

            if (items is not null)
            {
                Cart.Clear();
                Cart.AddRange(items);
            }

            var promo = await _session.GetItemAsync<Promotion>(PromotionKey);

            if (promo != null)
            {
                _appliedPromotion = promo;
            }
            OnChange?.Invoke();   // Báo cho UI biết giỏ đã có dữ liệu
        }

        public List<CartItemDTO> GetCart() => Cart;

        private async Task PersistAsync()
        {
            await _session.SetItemAsync(CartKey, Cart);
            if (_appliedPromotion != null)
            {
                await _session.SetItemAsync(PromotionKey, _appliedPromotion);
            }
        }

        public async Task AddToCartAsync(CartItemDTO item)
        {
            var existing = Cart.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existing != null)
            {
                if (item.SelectedQuantity <= (existing.Quantity - existing.SelectedQuantity))
                {
                    existing.SelectedQuantity += item.SelectedQuantity;
                }
                else
                {
                    existing.SelectedQuantity = item.SelectedQuantity;
                }
                existing.SelectedQuantityText = existing.SelectedQuantity.ToString();

            }
            else Cart.Add(item);

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
            Cart.Clear(); //empty cart item list
            _appliedPromotion = null;

            await _session.RemoveItemAsync(CartKey);
            await _session.RemoveItemAsync(PromotionKey);

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
        public Promotion? GetAppliedPromotion() => _appliedPromotion;

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

            await _session.SetItemAsync(PromotionKey, promo);

            OnChange?.Invoke();
        }

        // XOÁ MÃ (XOÁ SESSION)
        public async Task RemovePromotionAsync()
        {
            _appliedPromotion = null;

            await _session.RemoveItemAsync(PromotionKey);

            OnChange?.Invoke();
        }
    }
}
