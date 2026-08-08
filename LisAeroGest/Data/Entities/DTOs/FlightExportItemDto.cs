using System.Xml.Serialization;

namespace LisAeroGest.Data.Entities.DTOs
{
    public class FlightExportItemDto
    {
        [XmlElement("FlightId")]
        public int Id { get; set; }

        [XmlElement("FlightNumber")]
        public string FlightNumber { get; set; } = string.Empty;

        [XmlElement("Origin")]
        public string OriginAirport { get; set; } = string.Empty;

        [XmlElement("Destination")]
        public string DestinationAirport { get; set; } = string.Empty;

        [XmlElement("DepartureTime")]
        public DateTime DepartureTime { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }

        [XmlElement("TotalSeats")]
        public int TotalSeats { get; set; }

        [XmlElement("OccupiedSeats")]
        public int OccupiedSeats { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; } = string.Empty;
    }
}
