using StoreBlazor.Models;

namespace StoreBlazor.DTO.Client
{
    public class ProductCardDTO
    {
        public Product Product { get; set; } = new();
        public int Quantity { get; set; }
    }
}
