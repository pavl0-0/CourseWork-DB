using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kino.ViewModels
{
    public class SessionBatchDeleteViewModel
    {
        [Display(Name = "Фільм (не обов'язково)")]
        public int? FilmId { get; set; }

        [Display(Name = "Зал (не обов'язково)")]
        public int? HallId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "З дати")]
        public DateTime DateFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "По дату")]
        public DateTime DateTo { get; set; } = DateTime.Today.AddDays(7);

        public IEnumerable<SelectListItem>? Films { get; set; }
        public IEnumerable<SelectListItem>? Halls { get; set; }
    }
}