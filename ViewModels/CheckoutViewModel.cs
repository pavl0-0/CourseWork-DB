namespace Kino.ViewModels
{
    public class CheckoutViewModel
    {
        public int SessionId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public List<SelectedSeatInfo> SelectedSeats { get; set; } = new List<SelectedSeatInfo>();
        public string SelectedSeatIds { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientEmail { get; set; }
    }

    public class SelectedSeatInfo
    {
        public int Row { get; set; }
        public int Number { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}