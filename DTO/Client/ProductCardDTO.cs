using StoreBlazor.Models;

namespace StoreBlazor.DTO.Client
{
    public class ProductCardDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int CategoryId { get; set; }

        public decimal Price { get; set; }
        public string Unit { get; set; }

        public int Quantity { get; set; }
    }
}
