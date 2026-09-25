namespace LisAeroGest.Mobile.Models;

public class EmployeeFlightOperationDto
{
    public int FlightId { get; set; }

    public string FlightNumber { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }

    public string Gate { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int TotalPassengers { get; set; }

    public int CheckedIn { get; set; }

    public int PendingCheckIns { get; set; }

    public List<EmployeeFlightPassengerDto> Passengers { get; set; } = new();
}