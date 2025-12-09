namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO cho sản phẩm trong giỏ hàng
    /// </summary>
    public class CartItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductImage { get; set; } = string.Empty;

        /// <summary>
        /// Đơn giá
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách mua
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Tồn kho (để validate max quantity)
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Thành tiền = Price * Quantity
        /// </summary>
        public decimal Subtotal => Price * Quantity;
    }
}