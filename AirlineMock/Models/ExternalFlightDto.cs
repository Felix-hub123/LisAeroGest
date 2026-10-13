using System;

namespace LisAeroGest.AirlineMock.Models
{
    public class ExternalFlightDto
    {
        public string AirlineCode { get; set; } = string.Empty;

        public string FlightNumber { get; set; } = string.Empty;

        public DateTimeOffset ScheduledDepartureUtc { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset? EstimatedDepartureUtc { get; set; }

        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}