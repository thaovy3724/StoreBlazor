namespace StoreBlazor.DTO.Client
{
    public class OrderItemCreateClientDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        // Số lượng mua
        public int Quantity { get; set; }

        // Đơn giá tại thời điểm mua
        public decimal Price { get; set; }

        // Thành tiền = Price * Quantity
        public decimal SubTotal => Price * Quantity;
    }
}
