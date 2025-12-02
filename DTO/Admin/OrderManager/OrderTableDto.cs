using StoreBlazor.Models;

namespace StoreBlazor.DTO.Admin.OrderManager
{
    public class OrderTableDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
    }
}
