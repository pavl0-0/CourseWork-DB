using Kino.Data;
using Kino.Models;
using Kino.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kino.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Client model)
        {
            ModelState.Remove("Bookings");

            if (_context.Clients.Any(c => c.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Такий email вже зареєстрований");
            }

            if (_context.Clients.Any(c => c.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Цей номер телефону вже використовується");
            }

            if (ModelState.IsValid)
            {
                model.RegistrationDate = DateTime.Now;
                model.BonusPoints = 0;
                _context.Add(model);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("UserId", model.ClientId);
                HttpContext.Session.SetString("UserName", model.FirstName);
                HttpContext.Session.SetString("UserType", "Client");

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);

                if (client != null)
                {
                    HttpContext.Session.SetInt32("UserId", client.ClientId);
                    HttpContext.Session.SetString("UserName", client.FirstName);
                    HttpContext.Session.SetString("UserType", "Client");

                    return RedirectToAction("Profile");
                }

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == model.Email && e.PasswordHash == model.Password);

                if (employee != null)
                {
                    HttpContext.Session.SetInt32("UserId", employee.EmployeeId);
                    HttpContext.Session.SetString("UserName", employee.FirstName);
                    HttpContext.Session.SetString("UserType", "Admin");

                    return RedirectToAction("Index", "Bookings");
                }

                ModelState.AddModelError("", "Невірний логін або пароль");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Clients
                .Include(c => c.Bookings).ThenInclude(b => b.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Film)
                .Include(c => c.Bookings).ThenInclude(b => b.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Hall)
                .Include(c => c.Bookings).ThenInclude(b => b.Tickets).ThenInclude(t => t.Seat)
                .FirstOrDefaultAsync(c => c.ClientId == userId);

            user.Bookings = user.Bookings.OrderByDescending(b => b.BookingTime).ToList();

            return View(user);
        }
    }
}