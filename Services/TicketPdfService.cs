using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Kino.Services
{
    public class TicketData
    {
        public string FilmName { get; set; }
        public string HallName { get; set; }
        public DateTime Date { get; set; }
        public string SeatInfo { get; set; }
        public string SeatType { get; set; }
        public decimal Price { get; set; }
        public byte[] QrCodeImage { get; set; }
    }

    public class TicketPdfService
    {
        public byte[] GeneratePdf(List<TicketData> tickets)
        {
            var document = Document.Create(container =>
            {
                foreach (var ticket in tickets)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A6.Landscape());
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("MULTIPLEX").SemiBold().FontSize(18).FontColor(Colors.Red.Medium);
                                col.Item().Text(ticket.FilmName).FontSize(16).Bold().FontColor(Colors.Black);

                                col.Item().PaddingTop(10).Text(t =>
                                {
                                    t.Span("📅 Дата: ").FontColor(Colors.Grey.Darken1);
                                    t.Span($"{ticket.Date:dd.MM.yyyy}").SemiBold();
                                });

                                col.Item().Text(t =>
                                {
                                    t.Span("⏰ Час: ").FontColor(Colors.Grey.Darken1);
                                    t.Span($"{ticket.Date:HH:mm}").SemiBold();
                                    
                                });

                                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                                col.Item().PaddingTop(5).Text($"🏛 {ticket.HallName}").FontSize(12);
                                col.Item().Text($"💺 {ticket.SeatInfo}").FontSize(14).Bold();
                                col.Item().Text($"({ticket.SeatType})").FontSize(10).FontColor(Colors.Grey.Medium);

                                col.Item().PaddingTop(10).Text($"{ticket.Price:0} грн").FontSize(16).FontColor(Colors.Green.Medium).Bold();
                            });

                            row.ConstantItem(150).PaddingLeft(20).Column(col =>
                            {
                                col.Item().BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingLeft(10).Column(c =>
                                {
                                    c.Item().Text("КОНТРОЛЬ").FontSize(10).AlignCenter().FontColor(Colors.Grey.Medium);
                                    c.Item().PaddingTop(5).Image(ticket.QrCodeImage);
                                    c.Item().PaddingTop(5).Text("Сканувати на вході").FontSize(8).AlignCenter();
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Kino Multiplex | Дякуємо, що ви з нами!");
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }
    }
}