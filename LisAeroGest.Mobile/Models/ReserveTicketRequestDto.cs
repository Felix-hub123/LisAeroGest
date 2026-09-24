namespace LisAeroGest.Mobile.Models;

public class ReserveTicketRequestDto
{
    public int FlightId { get; set; }
    public int SeatId { get; set; }
    public bool ExtraLuggage { get; set; }
    public bool MealIncluded { get; set; }
}
