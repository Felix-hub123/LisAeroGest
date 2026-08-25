using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    public class PaymentViewModel
    {
        public int TicketId { get; set; }
        public string? FlightNumber { get; set; }
        public decimal FlightBasePrice { get; set; }
        public string? SeatCode { get; set; }
        public decimal SeatPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool ExtraLuggage { get; set; }
        public bool MealIncluded { get; set; }
        public string? PassengerName { get; set; }
        public string? PassengerEmail { get; set; }
        public List<Ticket> Tickets { get; set; } = new();
        
    }
}
