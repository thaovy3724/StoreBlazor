using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreBlazor.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("product_name")]
        public string ProductName { get; set; }

        [Column("product_image")]
        public string ProductImage { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("unit")]
        [MaxLength(20)]
        public string Unit { get; set; }

        [Column("barcode")]
        [MaxLength(50)]
        public string Barcode { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // bidrectional navigation properties
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        public Inventory Inventory { get; set; }
    }
}
