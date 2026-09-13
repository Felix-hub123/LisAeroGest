namespace LisAeroGest.Models
{
    public class FlightCalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public int FlightCount { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
    }
}
