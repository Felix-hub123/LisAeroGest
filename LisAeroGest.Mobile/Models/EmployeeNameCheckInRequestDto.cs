namespace LisAeroGest.Mobile.Models;

public class EmployeeNameCheckInRequestDto
{
    public string PassengerName { get; set; } = string.Empty;
    public string? FlightNumber { get; set; }
    public string? DocumentNumber { get; set; }
}
