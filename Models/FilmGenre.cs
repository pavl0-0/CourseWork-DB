using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kino.Models
{
    [PrimaryKey(nameof(FilmId), nameof(GenreId))]
    public class FilmGenre
    {
        public int FilmId { get; set; }
        [ForeignKey("FilmId")]
        public virtual Film Film { get; set; } = null!;

        public int GenreId { get; set; }
        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; } = null!;
    }
}