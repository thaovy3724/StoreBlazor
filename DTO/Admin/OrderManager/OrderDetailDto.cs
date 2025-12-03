using StoreBlazor.Models;

namespace StoreBlazor.DTO.Admin.OrderManager
{
    public class OrderDetailDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }

        // Thanh toán
        public PaymentMethod PaymentMethod { get; set; } 
        public decimal PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Giảm giá, tổng tiền
        public int? PromoId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PromoCode { get; set; }= string.Empty;

        // Danh sách sản phẩm
        public List<OrderItemDto> Items { get; set; } = new();

        public class OrderItemDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string ProductImage { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => Price * Quantity;
        }
    }
}
