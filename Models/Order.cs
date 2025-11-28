using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreBlazor.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("promo_id")]
        public int? PromoId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column("status")]
        [StringLength(10)]
        public OrderStatus Status { get; set; }
        // "pending" | "paid" | "canceled"

        [Required]
        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column("discount_amount", TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        // Navigation Properties
        // 1 order - 1 customer
        public Customer? Customer { get; set; }
        // 1 order - 1 user
        public User? User { get; set; }
        // 1 order - 1 promotion
        public Promotion? Promotion { get; set; }
        // 1 order - n order_items
        public ICollection<OrderItem>? OrderItems { get; set; }
        // 1 order - 1 payment
        public Payment Payment { get; set; }
    }
}
