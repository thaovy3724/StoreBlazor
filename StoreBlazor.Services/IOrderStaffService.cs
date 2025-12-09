using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
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
        Task<ServiceResult> CreateOrderAsync(OrderCreateDto orderDto, int userId);

        Task<ServiceResult> CreateOrderWithPaymentAsync(OrderCreateDto orderDto, int userId, string ipAddress);

        Task<ServiceResult> UpdateOrderStatusAfterPaymentAsync(int orderId, PaymentMethod paymentMethod, string transactionId);
    }
}