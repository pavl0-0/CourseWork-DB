using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kino.Models
{
    [PrimaryKey(nameof(FilmId), nameof(CountryId))]
    public class FilmCountry
    {
        public int FilmId { get; set; }
        [ForeignKey("FilmId")]
        public virtual Film Film { get; set; } = null!;

        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; } = null!;
    }
}