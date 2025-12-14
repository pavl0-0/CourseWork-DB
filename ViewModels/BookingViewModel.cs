namespace Kino.ViewModels
{
    public class BookingViewModel
    {
        public int SessionId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal BasePrice { get; set; }

        public List<SeatRowViewModel> Rows { get; set; } = new List<SeatRowViewModel>();
    }

    public class SeatRowViewModel
    {
        public int RowNumber { get; set; }
        public List<SeatViewModel> Seats { get; set; } = new List<SeatViewModel>();
    }

    public class SeatViewModel
    {
        public int SeatId { get; set; }
        public int Number { get; set; }
        public string Type { get; set; } = "Standard";
        public decimal PriceMultiplier { get; set; }
        public bool IsBooked { get; set; }
    }
}