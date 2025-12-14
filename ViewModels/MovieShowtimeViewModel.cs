namespace Kino.ViewModels
{
    public class MovieShowtimeViewModel
    {
        public int FilmId { get; set; }
        public string Title { get; set; }
        public string PosterUrl { get; set; }
        public int Duration { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int AgeRating { get; set; }
        public bool IsHighlighted { get; set; }

        public List<SessionItem> Sessions { get; set; }

        public List<CrewInfo> Crew { get; set; } = new List<CrewInfo>();
    }

    public class SessionItem
    {
        public int SessionId { get; set; }
        public string Time { get; set; }
        public string HallName { get; set; }
        public decimal Price { get; set; }
        public DateTime _SortTime { get; set; }
    }

    public class CrewInfo
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}