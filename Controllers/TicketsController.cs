using Kino.Data;
using Kino.Models;
using Kino.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kino.Services;
using QRCoder;

namespace Kino.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var expiredBookings = await _context.Bookings
            .Where(b => b.Status == "Pending" && b.BookingTime < DateTime.Now.AddMinutes(-15))
            .ToListAsync();

            if (expiredBookings.Any())
            {
                var expiredIds = expiredBookings.Select(b => b.BookingId).ToList();
                var ticketsToDelete = _context.Tickets.Where(t => expiredIds.Contains(t.BookingId));

                _context.Tickets.RemoveRange(ticketsToDelete);
                _context.Bookings.RemoveRange(expiredBookings);
                await _context.SaveChangesAsync();
            }
            var session = await _context.Sessions
                .Include(s => s.Film)
                .Include(s => s.Hall).ThenInclude(h => h.Seats).ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            var bookedSeatIds = await _context.Tickets
                .Where(t => t.SessionId == id)
                .Select(t => t.SeatId)
                .ToListAsync();

            var model = new BookingViewModel
            {
                SessionId = session.SessionId,
                MovieTitle = session.Film.TitleUkrainian,
                HallName = $"Зал №{session.Hall.HallNumber}",
                StartTime = session.StartTime,
                BasePrice = session.BaseTicketPrice,
                Rows = session.Hall.Seats
                    .GroupBy(s => s.RowNumber)
                    .OrderBy(g => g.Key)
                    .Select(row => new SeatRowViewModel
                    {
                        RowNumber = row.Key,
                        Seats = row.OrderBy(s => s.SeatNumber).Select(s => new SeatViewModel
                        {
                            SeatId = s.SeatId,
                            Number = s.SeatNumber,
                            Type = s.SeatType.Name,
                            PriceMultiplier = s.SeatType.PriceMultiplier,
                            IsBooked = bookedSeatIds.Contains(s.SeatId)
                        }).ToList()
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int sessionId, string selectedSeatIds)
        {
            if (string.IsNullOrEmpty(selectedSeatIds)) return RedirectToAction("Book", new { id = sessionId });

            var session = await _context.Sessions
                .Include(s => s.Film)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            var seats = await _context.Seats
                .Include(s => s.SeatType)
                .Where(s => seatIds.Contains(s.SeatId))
                .ToListAsync();

            var model = new CheckoutViewModel
            {
                SessionId = sessionId,
                MovieTitle = session.Film.TitleUkrainian,
                HallName = $"Зал №{session.Hall.HallNumber}",
                StartTime = session.StartTime,
                SelectedSeatIds = selectedSeatIds,
                SelectedSeats = seats.Select(s => new SelectedSeatInfo
                {
                    Row = s.RowNumber,
                    Number = s.SeatNumber,
                    Type = s.SeatType.Name,
                    Price = session.BaseTicketPrice * s.SeatType.PriceMultiplier
                }).ToList()
            };

            model.TotalPrice = model.SelectedSeats.Sum(s => s.Price);

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == userId);

                if (client != null)
                {
                    model.ClientName = $"{client.FirstName} {client.LastName}".Trim();
                    model.ClientPhone = client.PhoneNumber;
                    model.ClientEmail = client.Email;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int sessionId, string selectedSeatIds, string guestName, string guestPhone, string guestEmail, string paymentMethod)
        {

            if (string.IsNullOrEmpty(selectedSeatIds))
            {
                return RedirectToAction("Book", new { id = sessionId });
            }
            var seatIdsList = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            var alreadyBooked = await _context.Tickets
                .AnyAsync(t => t.SessionId == sessionId && seatIdsList.Contains(t.SeatId));

            if (alreadyBooked)
            {
                TempData["Error"] = "Ой! Хтось встиг купити ці місця раніше за вас.";
                return RedirectToAction("Book", new { id = sessionId });
            }
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == guestEmail);
            if (client == null)
            {
                client = new Client
                {
                    FirstName = guestName,
                    LastName = "",
                    PhoneNumber = guestPhone,
                    Email = guestEmail,
                    PasswordHash = "GUEST",
                    RegistrationDate = DateTime.Now
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            var booking = new Booking
            {
                BookingTime = DateTime.Now,
                Status = "Pending",
                ClientId = client.ClientId,
                TotalAmount = 0
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            var session = await _context.Sessions.FindAsync(sessionId);
            decimal totalPrice = 0;

            foreach (var seatId in seatIds)
            {
                var seat = await _context.Seats.Include(s => s.SeatType).FirstOrDefaultAsync(s => s.SeatId == seatId);
                var price = session.BaseTicketPrice * seat.SeatType.PriceMultiplier;
                totalPrice += price;

                _context.Tickets.Add(new Ticket
                {
                    BookingId = booking.BookingId,
                    SessionId = sessionId,
                    SeatId = seatId,
                    Price = price,
                    Status = "Active"
                });
            }

            booking.TotalAmount = totalPrice;
            await _context.SaveChangesAsync();

            return RedirectToAction("Payment", new { bookingId = booking.BookingId, method = paymentMethod });
        }

        public IActionResult Success(int id)
        {
            return View(id);
        }

        [HttpGet]
        public IActionResult Payment(int bookingId, string method)
        {
            ViewBag.Method = method;
            return View(bookingId);
        }

        [HttpPost]
        public async Task<IActionResult> FinishPayment(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Film)
                .Include(b => b.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Hall)
                .Include(b => b.Tickets).ThenInclude(t => t.Seat).ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();

            booking.Status = "Paid";
            await _context.SaveChangesAsync();

            var ticketsForPdf = new List<Kino.Services.TicketData>();
            using (var qrGenerator = new QRCodeGenerator())
            {
                foreach (var ticket in booking.Tickets)
                {
                    string uniqueQrData = $"TICKET-{booking.BookingId}-{ticket.TicketId}-{Guid.NewGuid().ToString().Substring(0, 4)}";

                    var qrData = qrGenerator.CreateQrCode(uniqueQrData, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrData);
                    byte[] qrBytes = qrCode.GetGraphic(20);

                    ticketsForPdf.Add(new Kino.Services.TicketData
                    {
                        FilmName = ticket.Session.Film.TitleUkrainian,
                        HallName = $"Зал {ticket.Session.Hall.HallNumber}",
                        Date = ticket.Session.StartTime,
                        SeatInfo = $"Ряд {ticket.Seat.RowNumber}, Місце {ticket.Seat.SeatNumber}",
                        SeatType = ticket.Seat.SeatType.Name,
                        Price = ticket.Price,
                        QrCodeImage = qrBytes
                    });
                }
            }

            var pdfService = new TicketPdfService();
            byte[] pdfFile = pdfService.GeneratePdf(ticketsForPdf);

            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                    <div style='max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #28a745;'>Оплата успішна!</h2>
                        <p>Вітаємо, {booking.Client.FirstName}!</p>
                        <p>Ви придбали <strong>{booking.Tickets.Count} квитків</strong> на фільм <strong>{booking.Tickets.First().Session.Film.TitleUkrainian}</strong>.</p>
                        <p>У вкладенні ви знайдете PDF-файл, де <strong>кожен квиток — на окремій сторінці</strong>.</p>
                        <p>Ви можете показати їх з екрану телефону по черзі.</p>
                        <hr/>
                        <p style='color: #888;'>До зустрічі в кіно!</p>
                    </div>
                </div>
            ";

            var emailService = new EmailService();
            try
            {
                await emailService.SendTicketAsync(booking.Client.Email, "Ваші квитки в кіно 🍿", emailBody, pdfFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка пошти: " + ex.Message);
            }

            return RedirectToAction("Success", new { id = booking.BookingId });
        }

        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                var tickets = _context.Tickets.Where(t => t.BookingId == bookingId);
                _context.Tickets.RemoveRange(tickets);

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CancelBookingAPI(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking != null && booking.Status == "Pending")
            {
                var tickets = _context.Tickets.Where(t => t.BookingId == bookingId);
                _context.Tickets.RemoveRange(tickets);
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
    }
}