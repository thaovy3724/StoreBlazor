namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO hiển thị sản phẩm trong danh sách (bên trái)
    /// </summary>
    public class ProductCardDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductImage { get; set; } = string.Empty;

        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng tồn kho
        /// </summary>
        public int Quantity { get; set; }
    }
}