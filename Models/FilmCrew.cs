using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kino.Models
{
    [PrimaryKey(nameof(FilmId), nameof(PersonId), nameof(RoleId))]
    public class FilmCrew
    {
        public int FilmId { get; set; }
        [ForeignKey("FilmId")]
        public virtual Film Film { get; set; } = null!;

        public int PersonId { get; set; }
        [ForeignKey("PersonId")]
        public virtual Person Person { get; set; } = null!;

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [MaxLength(100)]
        public string? CharacterName { get; set; }
    }
}