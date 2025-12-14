using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kino.Data;
using Kino.Models;

namespace Kino.Controllers
{
    public class HallsController : Controller
    {
        private readonly AppDbContext _context;

        public HallsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Halls
        public async Task<IActionResult> Index()
        {
            return View(await _context.Halls.ToListAsync());
        }

        // GET: Halls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.HallId == id);
            if (hall == null)
            {
                return NotFound();
            }

            return View(hall);
        }

        // GET: Halls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Halls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HallId,HallNumber,SeatCapacity,ScreenType,SoundType")] Hall hall)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hall);
        }

        // GET: Halls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                return NotFound();
            }
            return View(hall);
        }

        // POST: Halls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HallId,HallNumber,SeatCapacity,ScreenType,SoundType")] Hall hall)
        {
            if (id != hall.HallId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hall);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(hall.HallId))
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
            return View(hall);
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.HallId == id);
            if (hall == null)
            {
                return NotFound();
            }

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.HallId == id);
        }

        public async Task<IActionResult> GenerateSeats(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return NotFound();

            int autoWidth = hall.SeatCapacity <= 20 ? hall.SeatCapacity : 12;

            var model = new Kino.ViewModels.SeatGeneratorViewModel
            {
                HallId = hall.HallId,
                HallName = $"Зал №{hall.HallNumber} ({hall.ScreenType})",
                HallCapacity = hall.SeatCapacity,
                SeatsPerRow = autoWidth,

                SeatTypes = _context.SeatTypes.Select(st => new SelectListItem
                {
                    Value = st.SeatTypeId.ToString(),
                    Text = $"{st.Name} (x{st.PriceMultiplier})"
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSeats(Kino.ViewModels.SeatGeneratorViewModel model)
        {
            var oldSeats = _context.Seats.Where(s => s.HallId == model.HallId);
            _context.Seats.RemoveRange(oldSeats);
            await _context.SaveChangesAsync();

            int totalRows = (int)Math.Ceiling((double)model.HallCapacity / model.SeatsPerRow);

            var newSeats = new List<Seat>();
            int seatsCreated = 0;

            for (int row = 1; row <= totalRows; row++)
            {
                int currentTypeId = model.MainSeatTypeId;

                if (model.VipSeatTypeId.HasValue && row > (totalRows - model.VipRowsCount))
                {
                    currentTypeId = model.VipSeatTypeId.Value;
                }

                for (int number = 1; number <= model.SeatsPerRow; number++)
                {
                    if (seatsCreated >= model.HallCapacity) break;

                    newSeats.Add(new Seat
                    {
                        HallId = model.HallId,
                        RowNumber = row,
                        SeatNumber = number,
                        SeatTypeId = currentTypeId
                    });
                    seatsCreated++;
                }
            }

            await _context.Seats.AddRangeAsync(newSeats);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
