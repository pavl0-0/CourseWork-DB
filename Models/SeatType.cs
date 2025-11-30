using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class SeatType
    {
        [Key]
        public int SeatTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3, 2)")]
        public decimal PriceMultiplier { get; set; } = 1.00m;
    }
}