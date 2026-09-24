namespace LisAeroGest.Mobile.Models
{
    public class BoardingPassDto
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string FlightNumber { get; set; } = string.Empty;

        public string Gate { get; set; } = string.Empty;

        public int SequenceNumber { get; set; }

        public DateTime IssuedAt { get; set; }

        public string QRData { get; set; } = string.Empty;
    }
}