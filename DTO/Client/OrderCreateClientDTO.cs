using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.DTO.Client
{
    public class OrderCreateClientDTO
    {
        public int? UserId { get; set; }

        public int CustomerId { get; set; }

        public string Address { get; set; } = string.Empty;

        // ID khuyến mãi (nullable)
        public int? PromoId { get; set; }

        // Tổng tiền hàng (chưa giảm giá)
        public decimal TotalAmount { get; set; }

        // Số tiền được giảm
        public decimal DiscountAmount { get; set; }

        // Phương thức thanh toán
        public PaymentMethod PaymentMethod { get; set; }

        // Danh sách sản phẩm trong đơn
        public List<OrderItemCreateClientDTO> Items { get; set; } = new();
    }
}
