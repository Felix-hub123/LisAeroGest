namespace LisAeroGest.Mobile.Models;

public class EmployeeCheckInResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PassengerName { get; set; }
    public string? FlightNumber { get; set; }
    public string? Gate { get; set; }
    public int SequenceNumber { get; set; }
    public string? QrData { get; set; }
}
