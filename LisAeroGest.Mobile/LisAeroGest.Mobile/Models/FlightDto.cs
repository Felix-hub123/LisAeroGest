namespace LisAeroGest.Mobile.Models
{
    public class FlightDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AirlineCode { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DestinationCode { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}