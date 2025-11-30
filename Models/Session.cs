using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BaseTicketPrice { get; set; }

        public int FilmId { get; set; }
        [ForeignKey("FilmId")]
        public virtual Film Film { get; set; } = null!;

        public int HallId { get; set; }
        [ForeignKey("HallId")]
        public virtual Hall Hall { get; set; } = null!;
    }
}