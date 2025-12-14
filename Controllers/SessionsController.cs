using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kino.Data;
using Kino.Models;
using Kino.ViewModels;

namespace Kino.Controllers
{
    public class SessionsController : Controller
    {
        private readonly AppDbContext _context;

        public SessionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Date_Desc" : "";
            ViewData["FilmSortParm"] = sortOrder == "Film" ? "Film_Desc" : "Film";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "Price_Desc" : "Price";
            ViewData["HallSortParm"] = sortOrder == "Hall" ? "Hall_Desc" : "Hall";

            var sessions = _context.Sessions.Include(s => s.Film).Include(s => s.Hall).AsQueryable();

            switch (sortOrder)
            {
                case "Date_Desc":
                    sessions = sessions.OrderByDescending(s => s.StartTime);
                    break;
                case "Film":
                    sessions = sessions.OrderBy(s => s.Film.TitleUkrainian);
                    break;
                case "Film_Desc":
                    sessions = sessions.OrderByDescending(s => s.Film.TitleUkrainian);
                    break;
                case "Price":
                    sessions = sessions.OrderBy(s => s.BaseTicketPrice);
                    break;
                case "Price_Desc":
                    sessions = sessions.OrderByDescending(s => s.BaseTicketPrice);
                    break;
                case "Hall":
                    sessions = sessions.OrderBy(s => s.Hall.HallNumber);
                    break;
                case "Hall_Desc":
                    sessions = sessions.OrderByDescending(s => s.Hall.HallNumber);
                    break;
                default:
                    sessions = sessions.OrderBy(s => s.StartTime);
                    break;
            }

            return View(await sessions.ToListAsync());
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Film)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "TitleUkrainian");
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId");
            return View();
        }

        // POST: Sessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,StartTime,BaseTicketPrice,FilmId,HallId")] Session session)
        {
            ModelState.Remove("Film");
            ModelState.Remove("Hall");

            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "TitleUkrainian", session.FilmId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallNumber", session.HallId);
            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "TitleUkrainian", session.FilmId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId", session.HallId);
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,StartTime,BaseTicketPrice,FilmId,HallId")] Session session)
        {
            if (id != session.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "TitleUkrainian", session.FilmId);
            ViewData["HallId"] = new SelectList(_context.Halls, "HallId", "HallId", session.HallId);
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Film)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            bool hasSoldTickets = await _context.Tickets.AnyAsync(t => t.SessionId == id);

            if (hasSoldTickets)
            {
                TempData["Error"] = "Не можна видалити сеанс, на який вже продано квитки! Спочатку скасуйте замовлення.";
                return RedirectToAction(nameof(Index));
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.SessionId == id);
        }

        public IActionResult Generate()
        {
            var model = new SessionGeneratorViewModel
            {
                Films = _context.Films.Select(f => new SelectListItem { Value = f.FilmId.ToString(), Text = f.TitleUkrainian }),
                Halls = _context.Halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = $"Зал {h.HallNumber} ({h.ScreenType})" })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(SessionGeneratorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var timeStrings = model.Times.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var createdCount = 0;

                var film = await _context.Films.FindAsync(model.FilmId);
                if (film == null) return NotFound();

                foreach (var hallId in model.HallIds)
                {
                    for (var date = model.DateFrom; date <= model.DateTo; date = date.AddDays(1))
                    {
                        foreach (var timeStr in timeStrings)
                        {
                            if (TimeSpan.TryParse(timeStr.Trim(), out TimeSpan time))
                            {
                                DateTime start = date.Date + time;
                                DateTime end = start.AddMinutes(film.DurationMinutes + 20);

                                bool isBusy = await _context.Sessions
                                    .AnyAsync(s => s.HallId == hallId &&
                                                   ((start >= s.StartTime && start < s.StartTime.AddMinutes(s.Film.DurationMinutes + 20)) ||
                                                    (end > s.StartTime && end <= s.StartTime.AddMinutes(s.Film.DurationMinutes + 20)) ||
                                                    (start <= s.StartTime && end >= s.StartTime.AddMinutes(s.Film.DurationMinutes + 20))));

                                if (!isBusy)
                                {
                                    var session = new Session
                                    {
                                        FilmId = model.FilmId,
                                        HallId = hallId,
                                        BaseTicketPrice = model.Price,
                                        StartTime = start
                                    };
                                    _context.Add(session);
                                    createdCount++;
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Films = _context.Films.Select(f => new SelectListItem { Value = f.FilmId.ToString(), Text = f.TitleUkrainian });
            model.Halls = _context.Halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = $"Зал {h.HallNumber}" });
            return View(model);
        }

        public IActionResult BatchDelete()
        {
            var model = new SessionBatchDeleteViewModel
            {
                Films = _context.Films.Select(f => new SelectListItem { Value = f.FilmId.ToString(), Text = f.TitleUkrainian }),
                Halls = _context.Halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = $"Зал {h.HallNumber}" })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchDelete(SessionBatchDeleteViewModel model)
        {
            var sessionsToDelete = _context.Sessions.AsQueryable();

            sessionsToDelete = sessionsToDelete.Where(s => s.StartTime.Date >= model.DateFrom.Date && s.StartTime.Date <= model.DateTo.Date);

            if (model.FilmId.HasValue)
            {
                sessionsToDelete = sessionsToDelete.Where(s => s.FilmId == model.FilmId.Value);
            }

            if (model.HallId.HasValue)
            {
                sessionsToDelete = sessionsToDelete.Where(s => s.HallId == model.HallId.Value);
            }

            await _context.Tickets
                .Where(t => sessionsToDelete.Select(s => s.SessionId).Contains(t.SessionId))
                .ExecuteDeleteAsync();

            int deletedCount = await sessionsToDelete.ExecuteDeleteAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult BatchEdit()
        {
            var model = new SessionBatchEditViewModel
            {
                Films = _context.Films.Select(f => new SelectListItem { Value = f.FilmId.ToString(), Text = f.TitleUkrainian }),
                Halls = _context.Halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = $"Зал {h.HallNumber}" })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchEdit(SessionBatchEditViewModel model)
        {
            var sessionsQuery = _context.Sessions.AsQueryable();

            sessionsQuery = sessionsQuery.Where(s => s.StartTime.Date >= model.DateFrom.Date && s.StartTime.Date <= model.DateTo.Date);

            if (model.FilterFilmId.HasValue)
            {
                sessionsQuery = sessionsQuery.Where(s => s.FilmId == model.FilterFilmId.Value);
            }

            if (model.FilterHallId.HasValue)
            {
                sessionsQuery = sessionsQuery.Where(s => s.HallId == model.FilterHallId.Value);
            }

            var sessionsToUpdate = await sessionsQuery.ToListAsync();

            if (!sessionsToUpdate.Any())
            {
                TempData["Error"] = "Сеансів за вказаними критеріями не знайдено.";
                model.Films = _context.Films.Select(f => new SelectListItem { Value = f.FilmId.ToString(), Text = f.TitleUkrainian });
                model.Halls = _context.Halls.Select(h => new SelectListItem { Value = h.HallId.ToString(), Text = $"Зал {h.HallNumber}" });
                return View(model);
            }

            int count = 0;
            foreach (var session in sessionsToUpdate)
            {
                bool changed = false;

                if (model.NewPrice.HasValue && model.NewPrice.Value > 0)
                {
                    session.BaseTicketPrice = model.NewPrice.Value;
                    changed = true;
                }

                if (model.NewHallId.HasValue)
                {
                    session.HallId = model.NewHallId.Value;
                    changed = true;
                }

                if (changed) count++;
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Успішно оновлено {count} сеансів.";
            }
            else
            {
                TempData["Error"] = "Ви не вказали жодних змін (ціна або зал).";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
