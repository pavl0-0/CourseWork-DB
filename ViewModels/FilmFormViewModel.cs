using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kino.ViewModels
{
    public class FilmFormViewModel
    {
        public int FilmId { get; set; }

        [Required(ErrorMessage = "Введіть назву фільму")]
        [Display(Name = "Назва українською")]
        public string TitleUkrainian { get; set; } = string.Empty;

        [Display(Name = "Назва оригінальна")]
        public string? TitleOriginal { get; set; }

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Тривалість (хв)")]
        public int DurationMinutes { get; set; }

        [Required]
        [Display(Name = "Дата виходу")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.Today;

        [Display(Name = "Вік")]
        public string? AgeRating { get; set; }

        [Display(Name = "Трейлер (YouTube URL)")]
        public string? TrailerUrl { get; set; }

        [Display(Name = "Бюджет ($)")]
        public decimal Budget { get; set; }

        [Display(Name = "Постер фільму")]
        public IFormFile? PosterFile { get; set; }
        public string? CurrentPosterUrl { get; set; }

        [Display(Name = "Жанри")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> AllGenres { get; set; } = new List<SelectListItem>();

        [Display(Name = "Країни")]
        public List<int> SelectedCountryIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> AllCountries { get; set; } = new List<SelectListItem>();
    }
}