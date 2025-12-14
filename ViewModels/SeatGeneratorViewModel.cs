using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kino.ViewModels
{
    public class SeatGeneratorViewModel
    {
        public int HallId { get; set; }
        public string HallName { get; set; } = string.Empty;
        public int HallCapacity { get; set; }

        [Required]
        [Display(Name = "Місць у ряду (ширина залу)")]
        public int SeatsPerRow { get; set; }


        [Display(Name = "Основний тип місць")]
        public int MainSeatTypeId { get; set; }

        [Display(Name = "VIP тип (для задніх рядів)")]
        public int? VipSeatTypeId { get; set; }

        [Display(Name = "Скільки останніх рядів зробити VIP?")]
        public int VipRowsCount { get; set; } = 0;

        public IEnumerable<SelectListItem>? SeatTypes { get; set; }
    }
}