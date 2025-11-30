using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        [Required]
        public int RowNumber { get; set; }

        [Required]
        public int SeatNumber { get; set; }

        [Required]
        public int HallId { get; set; }

        [ForeignKey("HallId")]
        public virtual Hall Hall { get; set; } = null!;

        [Required]
        public int SeatTypeId { get; set; }

        [ForeignKey("SeatTypeId")]
        public virtual SeatType SeatType { get; set; } = null!;
    }
}