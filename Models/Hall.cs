using System.ComponentModel.DataAnnotations;

namespace Kino.Models
{
    public class Hall
    {
        [Key]
        public int HallId { get; set; }

        [Required]
        public int HallNumber { get; set; }

        public int SeatCapacity { get; set; }

        [MaxLength(50)]
        public string? ScreenType { get; set; }

        [MaxLength(50)]
        public string? SoundType { get; set; }

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}