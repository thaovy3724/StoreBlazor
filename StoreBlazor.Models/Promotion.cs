using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreBlazor.Models
{
    [Table("promotions")]
    public class Promotion
    {
        [Key]
        [Column("promo_id")]
        public int PromoId { get; set; }

        [Required]
        [Column("promo_code")]
        [StringLength(50)]
        public string PromoCode { get; set; }

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        [Column("discount_type")]
        [StringLength(10)]
        public DiscountType DiscountType { get; set; } // Percent, Fixed

        [Required]
        [Column("discount_value", TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Column("min_order_amount", TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column("usage_limit")]
        public int UsageLimit { get; set; } = 0;

        [Column("used_count")]
        public int UsedCount { get; set; } = 0;

        [Column("status")]
        [StringLength(10)]
        public PromotionStatus Status { get; set; }

        // Navigation Properties
        // 1 promotion - n orders
        public ICollection<Order>? Orders { get; set; }
    }
}
