using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;

        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; } = null!;

        public int SeatId { get; set; }
        [ForeignKey("SeatId")]
        public virtual Seat Seat { get; set; } = null!;
    }
}