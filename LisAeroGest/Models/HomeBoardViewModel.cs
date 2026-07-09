using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    public class HomeBoardViewModel
    {
        public IEnumerable<Flight> Departures { get; set; } = new List<Flight>();
        public IEnumerable<Flight> Arrivals { get; set; } = new List<Flight>();
        public int ActiveFlightsCount { get; set; }
        public int DisruptedFlightsCount { get; set; }
        public WeatherData? Weather { get; set; }
        public List<Announcement> Announcements { get; set; } = new();
    }

    public class Announcement
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Icon { get; set; } = "bi-megaphone";
        public string Color { get; set; } = "text-primary";
    }
}
