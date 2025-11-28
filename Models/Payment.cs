using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreBlazor.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("payment_date")]
        public DateTime PaymentDate { get; set; }

        [Column("payment_method")]
        [StringLength(20)]
        public PaymentMethod PaymentMethod { get; set; }

        public Order? Order { get; set; }
    }
}
