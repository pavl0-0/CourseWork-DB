namespace Kino.ViewModels
{
    public class AdminStatsViewModel
    {
        // 👇 Додаємо діапазон дат для фільтрації
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<FilmStat> TopFilms { get; set; } = new List<FilmStat>();
        public List<DailyStat> RevenueStats { get; set; } = new List<DailyStat>(); 
        public List<GenreStat> TopGenres { get; set; } = new List<GenreStat>();
        public List<ClientStat> TopClients { get; set; } = new List<ClientStat>();
    }

    public class FilmStat { public string Title { get; set; } public int TicketsSold { get; set; } }
    public class DailyStat { public DateTime Date { get; set; } public decimal TotalAmount { get; set; } }
    public class ClientStat { public string Name { get; set; } public decimal TotalSpent { get; set; } }
    public class GenreStat { public string GenreName { get; set; } public decimal TotalRevenue { get; set; } public int TicketsCount { get; set; } }
}