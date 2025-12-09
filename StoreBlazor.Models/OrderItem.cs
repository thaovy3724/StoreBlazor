using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreBlazor.Models
{

    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")]
        public int OrderItemId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        // Navigation Properties
        // 1 order_item - 1 order
        public Order? Order { get; set; }
        // 1 order_item - 1 product
        public Product? Product { get; set; }
    }
}
