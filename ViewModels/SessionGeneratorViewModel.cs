using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kino.ViewModels
{
    public class SessionGeneratorViewModel
    {
        [Display(Name = "Фільм")]
        public int FilmId { get; set; }

        [Display(Name = "Зали (оберіть кілька)")]
        public List<int> HallIds { get; set; } = new List<int>();

        [Display(Name = "Ціна квитка")]
        public decimal Price { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "З дати")]
        public DateTime DateFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "По дату")]
        public DateTime DateTo { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Час сеансів (через кому)")]
        [Required]
        public string Times { get; set; } = "10:00, 14:00, 18:00";

        public IEnumerable<SelectListItem>? Films { get; set; }
        public IEnumerable<SelectListItem>? Halls { get; set; }
    }
}