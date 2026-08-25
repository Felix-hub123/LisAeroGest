namespace LisAeroGest.Models
{
    public class FlightDetailViewModel
    {
        public int Id { get; set; }

        public string FlightNumber { get; set; } = string.Empty;

        public string Airline { get; set; } = string.Empty;

        public string Origin { get; set; } = string.Empty;

        public string OriginCity { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public string DestinationCity { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public DateTime? EstimatedTime { get; set; }

        public string Gate { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;

        public string AircraftType { get; set; } = string.Empty;

        public int TotalSeats { get; set; }

        public int AvailableSeats { get; set; }

        public bool IsDelayed { get; set; }

        public int? DelayMinutes { get; set; }

        public List<FlightStatusHistory> StatusHistory { get; set; } = new();


        /// <summary>
        /// Histórico de estados de um voo
        /// </summary>
        public class FlightStatusHistory
        {
            public DateTime Timestamp { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
        }
    }
}
