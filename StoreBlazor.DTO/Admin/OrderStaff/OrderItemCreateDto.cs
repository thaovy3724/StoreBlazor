namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO cho chi tiết sản phẩm trong đơn hàng
    /// </summary>
    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng mua
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Đơn giá tại thời điểm mua
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Thành tiền = Price * Quantity
        /// </summary>
        public decimal Subtotal => Price * Quantity;
    }
}