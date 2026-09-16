namespace LisAeroGest.Mobile.Models
{
    public class FlightDetailDto
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AirlineCode { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string OriginCode { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DestinationCode { get; set; } = string.Empty;
        public string AircraftModel { get; set; } = string.Empty;
        public string Gate { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }

        public string DurationLabel =>
            DurationMinutes > 0
                ? $"{DurationMinutes / 60}h {DurationMinutes % 60:00}m"
                : "Não disponível";

        public string PriceLabel => BasePrice.ToString("C2");
    }
}
