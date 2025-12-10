using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IOrderStaffService
    {
        // Sản phẩm
        Task<PageResult<ProductCardDto>> GetProductsAsync(int page, int? categoryId = null, string? search = null);
        Task<ProductCardDto?> GetProductByBarcodeAsync(string barcode);

        // Category
        Task<List<Category>> GetAllCategoriesAsync();

        // Khách hàng
        Task<List<CustomerSuggestionDto>> SearchCustomerAsync(string keyword);
        Task<ServiceResult> CreateCustomerAsync(Customer customer);

        // Khuyến mãi
        Task<List<PromotionDto>> GetActivePromotionsAsync();

        // Đặt hàng
        Task<ServiceResult> CreateOrderWithPaymentAsync(OrderCreateDto orderDto);

        Task<ServiceResult> UpdateOrderStatusAfterPaymentAsync(int orderId, PaymentMethod paymentMethod);
    }
}