using StoreBlazor.Models;

namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO để tạo đơn hàng mới
    /// </summary>
    public class OrderCreateDto
    {
        public int? UserId { get; set; }

        /// <summary>
        /// ID khách hàng (nullable - khách vãng lai không cần)
        /// </summary>
        public int? CustomerId { get; set; }

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
        public List<OrderItemCreateDto> Items { get; set; } = new();

        // ===== COMPUTED PROPERTIES =====

        /// <summary>
        /// Số tiền khách cần thanh toán = TotalAmount - DiscountAmount
        /// </summary>
        public decimal FinalAmount => TotalAmount - DiscountAmount;

        /// <summary>
        /// Số lượng sản phẩm trong đơn
        /// </summary>
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}