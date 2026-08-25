using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    public class HomeBoardViewModel
    {
        public IEnumerable<Flight> Departures { get; set; } = new List<Flight>();

        public IEnumerable<Flight> Arrivals { get; set; } = new List<Flight>();

        public IEnumerable<FlightDetailViewModel> DepartureDetails { get; set; } = new List<FlightDetailViewModel>();
        public IEnumerable<FlightDetailViewModel> ArrivalDetails { get; set; } = new List<FlightDetailViewModel>();


        public int TotalPartidas => Departures.Count();

        public int TotalChegadas => Arrivals.Count();

        public int ActiveFlightsCount { get; set; }

        public int DisruptedFlightsCount { get; set; }


        // Novas propriedades

        public List<PopularDestination> PopularDestinations { get; set; } = new();

        public List<FlightWarning> ActiveWarnings { get; set; } = new();


        public WeatherData? Weather { get; set; }

        public List<Announcement> Announcements { get; set; } = new();

        public int TotalPartidasFuturas => Departures?.Count() ?? 0;
        public int TotalChegadasFuturas => Arrivals?.Count() ?? 0;

    }


    public class PopularDestination

    {

        public string IATA { get; set; } = "";

        public string Cidade { get; set; } = "";

        public string Pais { get; set; } = "";

        public int Voos { get; set; }

    }




    public class FlightWarning

    {

        public string FlightNumber { get; set; } = "";

        public string Origin { get; set; } = "";

        public string Destination { get; set; } = "";

        public string Status { get; set; } = "";

    }


    public class Announcement

    {

        public string Title { get; set; } = "";

        public string Message { get; set; } = "";

        public string Icon { get; set; } = "bi-megaphone";

        public string Color { get; set; } = "text-primary";

    }


}
