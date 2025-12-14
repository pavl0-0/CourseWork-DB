using Kino.Data;
using Kino.Models;
using Kino.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Kino.Controllers
{
    public class FilmsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FilmsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var films = await _context.Films
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .ToListAsync();
            return View(films);
        }

        public IActionResult Create()
        {
            var viewModel = new FilmFormViewModel
            {
                AllGenres = _context.Genres.Select(g => new SelectListItem { Value = g.GenreId.ToString(), Text = g.Name }).ToList(),
                AllCountries = _context.Countries.Select(c => new SelectListItem { Value = c.CountryId.ToString(), Text = c.Name }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                if (model.PosterFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/posters");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PosterFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PosterFile.CopyToAsync(fileStream);
                    }
                }

                var film = new Film
                {
                    TitleUkrainian = model.TitleUkrainian,
                    TitleOriginal = model.TitleOriginal,
                    Description = model.Description,
                    DurationMinutes = model.DurationMinutes,
                    ReleaseDate = model.ReleaseDate,
                    AgeRating = model.AgeRating,
                    TrailerUrl = model.TrailerUrl,
                    Budget = model.Budget,
                    PosterUrl = uniqueFileName
                };

                foreach (var genreId in model.SelectedGenreIds)
                {
                    film.FilmGenres.Add(new FilmGenre { GenreId = genreId });
                }
                foreach (var countryId in model.SelectedCountryIds)
                {
                    film.FilmCountries.Add(new FilmCountry { CountryId = countryId });
                }

                _context.Add(film);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.AllGenres = _context.Genres.Select(g => new SelectListItem { Value = g.GenreId.ToString(), Text = g.Name }).ToList();
            model.AllCountries = _context.Countries.Select(c => new SelectListItem { Value = c.CountryId.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .Include(f => f.FilmGenres)
                .Include(f => f.FilmCountries)
                .FirstOrDefaultAsync(f => f.FilmId == id);

            if (film == null) return NotFound();

            var viewModel = new FilmFormViewModel
            {
                FilmId = film.FilmId,
                TitleUkrainian = film.TitleUkrainian,
                TitleOriginal = film.TitleOriginal,
                Description = film.Description,
                DurationMinutes = film.DurationMinutes,
                ReleaseDate = film.ReleaseDate,
                AgeRating = film.AgeRating,
                TrailerUrl = film.TrailerUrl,
                Budget = film.Budget,
                CurrentPosterUrl = film.PosterUrl,

                SelectedGenreIds = film.FilmGenres.Select(fg => fg.GenreId).ToList(),
                SelectedCountryIds = film.FilmCountries.Select(fc => fc.CountryId).ToList(),

                AllGenres = _context.Genres.Select(g => new SelectListItem { Value = g.GenreId.ToString(), Text = g.Name }).ToList(),
                AllCountries = _context.Countries.Select(c => new SelectListItem { Value = c.CountryId.ToString(), Text = c.Name }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FilmFormViewModel model)
        {
            if (id != model.FilmId) return NotFound();

            if (ModelState.IsValid)
            {
                var filmToUpdate = await _context.Films
                    .Include(f => f.FilmGenres)
                    .Include(f => f.FilmCountries)
                    .FirstOrDefaultAsync(f => f.FilmId == id);

                if (filmToUpdate == null) return NotFound();

                filmToUpdate.TitleUkrainian = model.TitleUkrainian;
                filmToUpdate.TitleOriginal = model.TitleOriginal;
                filmToUpdate.Description = model.Description;
                filmToUpdate.DurationMinutes = model.DurationMinutes;
                filmToUpdate.ReleaseDate = model.ReleaseDate;
                filmToUpdate.AgeRating = model.AgeRating;
                filmToUpdate.TrailerUrl = model.TrailerUrl;
                filmToUpdate.Budget = model.Budget;

                if (model.PosterFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/posters");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PosterFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PosterFile.CopyToAsync(fileStream);
                    }
                    filmToUpdate.PosterUrl = uniqueFileName;
                }

                filmToUpdate.FilmGenres.Clear();
                foreach (var genreId in model.SelectedGenreIds)
                {
                    filmToUpdate.FilmGenres.Add(new FilmGenre { GenreId = genreId });
                }

                filmToUpdate.FilmCountries.Clear();
                foreach (var countryId in model.SelectedCountryIds)
                {
                    filmToUpdate.FilmCountries.Add(new FilmCountry { CountryId = countryId });
                }

                _context.Update(filmToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.AllGenres = _context.Genres.Select(g => new SelectListItem { Value = g.GenreId.ToString(), Text = g.Name }).ToList();
            model.AllCountries = _context.Countries.Select(c => new SelectListItem { Value = c.CountryId.ToString(), Text = c.Name }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(f => f.FilmCountries).ThenInclude(fc => fc.Country)
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Person)
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Role)
                .Include(f => f.Sessions).ThenInclude(s => s.Hall)

                .FirstOrDefaultAsync(m => m.FilmId == id);

            if (film == null) return NotFound();

            return View(film);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .FirstOrDefaultAsync(m => m.FilmId == id);
            if (film == null) return NotFound();

            return View(film);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }

            bool hasTickets = await _context.Tickets
                .Include(t => t.Session)
                .AnyAsync(t => t.Session.FilmId == id);

            if (hasTickets)
            {
                TempData["Error"] = "Неможливо видалити фільм, оскільки на нього вже є продані квитки! Спочатку скасуйте замовлення.";
                return RedirectToAction(nameof(Index));
            }

            var sessions = _context.Sessions.Where(s => s.FilmId == id);
            _context.Sessions.RemoveRange(sessions);

            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageCrew(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Person)
                .Include(f => f.FilmCrew).ThenInclude(fc => fc.Role)
                .FirstOrDefaultAsync(f => f.FilmId == id);

            if (film == null) return NotFound();

            var model = new Kino.ViewModels.FilmCrewViewModel
            {
                FilmId = film.FilmId,
                FilmTitle = film.TitleUkrainian,

                ExistingCrew = film.FilmCrew.Select(fc => new Kino.ViewModels.CrewItem
                {
                    PersonId = fc.PersonId,
                    FullName = fc.Person.FullName,
                    RoleId = fc.RoleId,
                    RoleName = fc.Role.RoleName  
                }).ToList(),

                PersonsList = _context.Persons.Select(p => new SelectListItem { Value = p.PersonId.ToString(), Text = p.FullName }),
                RolesList = _context.Roles.Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.RoleName })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCrewMember(Kino.ViewModels.FilmCrewViewModel model)
        {
            var exists = await _context.FilmCrew
                .AnyAsync(fc => fc.FilmId == model.FilmId && fc.PersonId == model.PersonId && fc.RoleId == model.RoleId);

            if (!exists)
            {
                var newMember = new FilmCrew
                {
                    FilmId = model.FilmId,
                    PersonId = model.PersonId,
                    RoleId = model.RoleId
                };
                _context.FilmCrew.Add(newMember);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageCrew), new { id = model.FilmId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCrewMember(int filmId, int personId, int roleId)
        {
            var record = await _context.FilmCrew
                .FirstOrDefaultAsync(fc => fc.FilmId == filmId && fc.PersonId == personId && fc.RoleId == roleId);

            if (record != null)
            {
                _context.FilmCrew.Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageCrew), new { id = filmId });
        }
    }
}