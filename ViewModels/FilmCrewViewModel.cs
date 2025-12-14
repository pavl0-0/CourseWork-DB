using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kino.ViewModels
{
    public class FilmCrewViewModel
    {
        public int FilmId { get; set; }
        public string FilmTitle { get; set; } = string.Empty;

        public int PersonId { get; set; }
        public int RoleId { get; set; }

        public IEnumerable<SelectListItem>? PersonsList { get; set; }
        public IEnumerable<SelectListItem>? RolesList { get; set; }

        public List<CrewItem> ExistingCrew { get; set; } = new List<CrewItem>();
    }

    public class CrewItem
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}