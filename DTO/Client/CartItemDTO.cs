namespace StoreBlazor.DTO.Client
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }
        public int SelectedQuantity { get; set; }
        public string SelectedQuantityText { get; set; } = "1"; // giá trị đang gõ
    }
}
