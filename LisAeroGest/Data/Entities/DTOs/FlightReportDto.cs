using System.Xml.Serialization;

namespace LisAeroGest.Data.Entities.DTOs
{

    [XmlRoot("FlightReport")]
    public class FlightReportDto
    {
        [XmlElement("GeneratedAt")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [XmlArray("Flights")]
        [XmlArrayItem("Flight")]
        public List<FlightExportItemDto> Flights { get; set; } = new();

    }
}
