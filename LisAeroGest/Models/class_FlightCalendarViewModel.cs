namespace LisAeroGest.Models
{
    public class FlightCalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public List<FlightCalendarDayViewModel> Days { get; set; } = new();
        public DateTime? SelectedDate { get; set; }
        public List<FlightSearchItemViewModel> FlightsOfDay { get; set; } = new();
    }
}
