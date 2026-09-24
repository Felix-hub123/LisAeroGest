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
        public string Gate { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Aircraft { get; set; } = string.Empty;

        public string AircraftModel =>
            string.IsNullOrWhiteSpace(Aircraft) ? string.Empty : Aircraft;

        public string PriceLabel =>
            BasePrice > 0
                ? BasePrice.ToString("C", new System.Globalization.CultureInfo("pt-PT"))
                : string.Empty;

        public string DurationLabel
        {
            get
            {
                if (ArrivalTime == default || DepartureTime == default)
                    return string.Empty;

                var duration = ArrivalTime - DepartureTime;
                if (duration.TotalMinutes <= 0)
                    return string.Empty;

                return duration.TotalHours >= 1
                    ? $"{(int)duration.TotalHours}h {duration.Minutes:00}m"
                    : $"{duration.Minutes} min";
            }
        }
    }
}