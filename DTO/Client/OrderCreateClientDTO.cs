using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.DTO.Client
{
    public class OrderCreateClientDTO
    {
        public int UserId { get; set; }

        public int CustomerId { get; set; }

        /// <summary>
        /// ID khuyến mãi (nullable)
        /// </summary>
        public int? PromoId { get; set; }

        /// <summary>
        /// Tổng tiền hàng (chưa giảm giá)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền được giảm
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Phương thức thanh toán
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Danh sách sản phẩm trong đơn
        /// </summary>
        public List<OrderItemCreateClientDTO> Items { get; set; } = new();
    }
}
