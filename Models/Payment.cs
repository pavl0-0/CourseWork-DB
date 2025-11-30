using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Card";

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Success";

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
    }
}