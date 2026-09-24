namespace LisAeroGest.Mobile.Models
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public bool ExtraLuggage { get; set; }
        public bool MealIncluded { get; set; }
        public int? BoardingPassId { get; set; }
    }
}