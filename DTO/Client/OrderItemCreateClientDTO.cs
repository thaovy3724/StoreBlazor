namespace StoreBlazor.DTO.Client
{
    public class OrderItemCreateClientDTO
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
        public decimal SubTotal => Price * Quantity;
    }
}
