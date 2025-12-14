using Kino.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kino.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Kino.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private async Task<AdminStatsViewModel> GetStats(DateTime start, DateTime end)
        {
            var model = new AdminStatsViewModel
            {
                StartDate = start,
                EndDate = end
            };

            model.TopFilms = await _context.Tickets
                .Include(t => t.Session).ThenInclude(s => s.Film)
                .Include(t => t.Booking)
                .Where(t => t.Booking.Status == "Paid" &&
                            t.Booking.BookingTime >= start && t.Booking.BookingTime <= end)
                .GroupBy(t => t.Session.Film.TitleUkrainian)
                .Select(g => new FilmStat { Title = g.Key, TicketsSold = g.Count() })
                .OrderByDescending(x => x.TicketsSold)
                .Take(5)
                .ToListAsync();

            model.RevenueStats = await _context.Bookings
                .Where(b => b.Status == "Paid" &&
                            b.BookingTime >= start && b.BookingTime <= end)
                .GroupBy(b => b.BookingTime.Date)
                .Select(g => new DailyStat { Date = g.Key, TotalAmount = g.Sum(b => b.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var ticketsWithGenres = await _context.Tickets
                .Include(t => t.Session).ThenInclude(s => s.Film).ThenInclude(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(t => t.Booking)
                .Where(t => t.Booking.Status == "Paid" &&
                            t.Booking.BookingTime >= start && t.Booking.BookingTime <= end)
                .ToListAsync();

            model.TopGenres = ticketsWithGenres
                .SelectMany(t => t.Session.Film.FilmGenres.Select(fg => new
                {
                    Genre = fg.Genre.Name,
                    Price = t.Price
                }))
                .GroupBy(x => x.Genre)
                .Select(g => new GenreStat
                {
                    GenreName = g.Key,
                    TotalRevenue = g.Sum(x => x.Price),
                    TicketsCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            model.TopClients = await _context.Bookings
                .Where(b => b.Status == "Paid" &&
                            b.BookingTime >= start && b.BookingTime <= end)
                .GroupBy(b => new { b.Client.FirstName, b.Client.LastName })
                .Select(g => new ClientStat
                {
                    Name = $"{g.Key.FirstName} {g.Key.LastName}",
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            return model;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin") return RedirectToAction("Login", "Account");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var stats = await GetStats(start, end);
            ViewBag.Stats = stats;

            ViewBag.FilmsCount = await _context.Films.CountAsync();
            ViewBag.SessionsCount = await _context.Sessions.CountAsync();
            ViewBag.ClientsCount = await _context.Clients.CountAsync();
            ViewBag.TodaySales = await _context.Bookings
                .Where(b => b.BookingTime.Date == DateTime.Today && b.Status == "Paid")
                .SumAsync(b => b.TotalAmount);

            return View();
        }

        public async Task<IActionResult> DownloadReport(DateTime? startDate, DateTime? endDate)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Login", "Account");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var stats = await GetStats(start, end);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MULTIPLEX").SemiBold().FontSize(24).FontColor(Colors.Red.Medium);
                            col.Item().Text("Система управління кінотеатром").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);
                            col.Item().Text($"Період звіту:").SemiBold().FontSize(10);
                            col.Item().Text($"{start:dd.MM.yyyy} — {end:dd.MM.yyyy}").FontSize(10).FontColor(Colors.Blue.Medium);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text("Аналітичний звіт").FontSize(18).Bold().Underline().AlignCenter();
                        col.Spacing(25);

                        col.Item().Text("1. Динаміка доходу").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(120); 
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Дата");
                                header.Cell().Element(CellStyle).AlignRight().Text("Виручка");
                            });

                            foreach (var item in stats.RevenueStats)
                            {
                                table.Cell().Element(CellStyle).Text(item.Date.ToString("dd.MM.yyyy (dddd)"));
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalAmount:N0} ₴");
                            }

                            table.Footer(footer =>
                            {
                                footer.Cell().Element(CellStyle).Text("РАЗОМ:").Bold();
                                footer.Cell().Element(CellStyle).AlignRight().Text($"{stats.RevenueStats.Sum(x => x.TotalAmount):N0} ₴").Bold();
                            });
                        });

                        col.Spacing(20);

                        col.Item().Text("2. Топ фільмів за продажами").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Назва фільму");
                                header.Cell().Element(CellStyle).AlignRight().Text("Продано квитків");
                            });

                            foreach (var item in stats.TopFilms)
                            {
                                table.Cell().Element(CellStyle).Text(item.Title);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.TicketsSold.ToString());
                            }
                        });

                        col.Spacing(20);

                        col.Item().Text("3. Ефективність жанрів").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Жанр");
                                header.Cell().Element(CellStyle).AlignRight().Text("Квитків");
                                header.Cell().Element(CellStyle).AlignRight().Text("Дохід");
                            });

                            foreach (var item in stats.TopGenres)
                            {
                                table.Cell().Element(CellStyle).Text(item.GenreName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.TicketsCount.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalRevenue:N0} ₴");
                            }
                        });

                        col.Spacing(20);

                        col.Item().Text("4. Топ клієнтів").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("ПІБ Клієнта");
                                header.Cell().Element(CellStyle).AlignRight().Text("Витрачено");
                            });

                            foreach (var item in stats.TopClients)
                            {
                                table.Cell().Element(CellStyle).Text(item.Name);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalSpent:N0} ₴");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Сторінка ");
                        x.CurrentPageNumber();
                        x.Span(" з ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"KinoReport_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf");
        }

        static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }
    }
}