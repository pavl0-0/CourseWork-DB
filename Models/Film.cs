using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kino.Models
{
    public class Film
    {
        [Key]
        public int FilmId { get; set; }

        [Required(ErrorMessage = "Назва обов'язкова")]
        [MaxLength(100)]
        public string TitleUkrainian { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TitleOriginal { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 1000)]
        public int DurationMinutes { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [MaxLength(10)]
        public string? AgeRating { get; set; }

        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }

        [Column(TypeName = "decimal(15, 2)")]
        public decimal Budget { get; set; }

        public virtual ICollection<FilmGenre> FilmGenres { get; set; } = new List<FilmGenre>();
        public virtual ICollection<FilmCountry> FilmCountries { get; set; } = new List<FilmCountry>();
        public virtual ICollection<FilmCrew> FilmCrew { get; set; } = new List<FilmCrew>();

        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}