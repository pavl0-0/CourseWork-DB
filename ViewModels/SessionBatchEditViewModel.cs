using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Kino.ViewModels
{
    public class SessionBatchEditViewModel
    {
        [Display(Name = "Фільм (залиште пустим для всіх)")]
        public int? FilterFilmId { get; set; }

        [Display(Name = "Зал (залиште пустим для всіх)")]
        public int? FilterHallId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "З дати")]
        public DateTime DateFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "По дату")]
        public DateTime DateTo { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Нова ціна (залиште 0, щоб не змінювати)")]
        public decimal? NewPrice { get; set; }

        [Display(Name = "Перенести в інший зал (опціонально)")]
        public int? NewHallId { get; set; }

        public IEnumerable<SelectListItem>? Films { get; set; }
        public IEnumerable<SelectListItem>? Halls { get; set; }
    }
}