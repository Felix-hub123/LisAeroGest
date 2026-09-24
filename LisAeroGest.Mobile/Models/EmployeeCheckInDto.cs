namespace LisAeroGest.Mobile.Models;

public class EmployeeCheckInDto
{
    public int TicketId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DocumentNumber { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Gate { get; set; } = "TBA";
    public string? Seat { get; set; }
    public bool CanCheckIn { get; set; }
}
