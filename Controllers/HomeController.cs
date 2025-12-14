using Kino.Data;
using Kino.Models;
using Kino.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Kino.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date, string searchQuery, string sortOrder, int? ageRating, decimal? maxPrice)
        {
            var selectedDate = date ?? DateTime.Today;

            var query = _context.Films
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Person)
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Role)
                .Include(f => f.Sessions).ThenInclude(s => s.Hall)
                .Where(f => f.Sessions.Any(s => s.StartTime >= DateTime.Today))
                .AsQueryable();

            var films = await query.ToListAsync();

            bool hasActiveFilters = !string.IsNullOrEmpty(searchQuery)
                                    || ageRating.HasValue
                                    || maxPrice.HasValue;

            var model = films.Select(f => {
                bool matchesAllFilters = true;

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    bool nameMatch = f.TitleUkrainian.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                     f.TitleOriginal.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                    if (!nameMatch) matchesAllFilters = false;
                }

                if (ageRating.HasValue)
                {
                    string cleanRating = new string(f.AgeRating?.Where(char.IsDigit).ToArray());
                    int.TryParse(cleanRating, out int dbAge);

                    if (dbAge > ageRating.Value) matchesAllFilters = false;
                }

                if (maxPrice.HasValue)
                {
                    bool priceMatch = f.Sessions.Any(s =>
                        s.StartTime.Date == selectedDate &&
                        (selectedDate == DateTime.Today ? s.StartTime > DateTime.Now : true) &&
                        s.BaseTicketPrice <= maxPrice.Value);

                    if (!priceMatch) matchesAllFilters = false;
                }

                bool isHighlighted = hasActiveFilters && matchesAllFilters;

                int.TryParse(new string(f.AgeRating?.Where(char.IsDigit).ToArray()), out int parsedAgeForView);

                return new MovieShowtimeViewModel
                {
                    FilmId = f.FilmId,
                    Title = f.TitleUkrainian,
                    PosterUrl = f.PosterUrl,
                    Duration = f.DurationMinutes,
                    Genre = f.FilmGenres.FirstOrDefault()?.Genre.Name ?? "Кіно",
                    ReleaseDate = f.ReleaseDate,
                    AgeRating = parsedAgeForView,

                    IsHighlighted = isHighlighted,

                    Sessions = f.Sessions
                        .Where(s => s.StartTime.Date == selectedDate)
                        .Where(s => selectedDate == DateTime.Today ? s.StartTime > DateTime.Now : true)
                        .OrderBy(s => s.StartTime)
                        .Select(s => new SessionItem
                        {
                            SessionId = s.SessionId,
                            Time = s.StartTime.ToString("HH:mm"),
                            HallName = s.Hall.ScreenType ?? "2D",
                            Price = s.BaseTicketPrice,
                            _SortTime = s.StartTime
                        })
                        .ToList(),

                    Crew = f.FilmCrew.Select(fc => new CrewInfo
                    {
                        Name = fc.Person.FullName,
                        Role = fc.Role.RoleName
                    }).ToList()
                };
            }).ToList();

            var orderedModel = model.OrderByDescending(m => m.IsHighlighted);

            switch (sortOrder)
            {
                case "newest":
                    model = orderedModel.ThenByDescending(m => m.ReleaseDate).ToList();
                    break;
                case "title":
                    model = orderedModel.ThenBy(m => m.Title).ToList();
                    break;
                case "price_asc":
                    model = orderedModel.ThenBy(m => m.Sessions.FirstOrDefault()?.Price ?? decimal.MaxValue).ToList();
                    break;
                default:
                    model = orderedModel
                        .ThenByDescending(m => m.Sessions.Any())
                        .ThenBy(m => m.Sessions.FirstOrDefault()?._SortTime ?? DateTime.MaxValue)
                        .ToList();
                    break;
            }

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortOrder = sortOrder;
            ViewBag.AgeRating = ageRating;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.HasActiveFilters = hasActiveFilters;

            return View(model);
        }

        public IActionResult Privacy() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}