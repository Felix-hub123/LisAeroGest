namespace LisAeroGest.Models
{
    public class PendingBookingDto
    {
        public int FlightId { get; set; }
        public int SeatId { get; set; }
        public List<int> ExtraIds { get; set; } = new List<int>();
        public decimal TotalPrice { get; set; }
    }
}
