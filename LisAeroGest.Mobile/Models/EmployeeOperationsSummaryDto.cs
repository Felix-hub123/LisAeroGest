namespace LisAeroGest.Mobile.Models;

public class EmployeeOperationsSummaryDto
{
    public int FlightCount { get; set; }
    public int TotalPassengers { get; set; }
    public int TotalTickets { get; set; }
    public int PendingCheckIns { get; set; }
    public int CheckedIn { get; set; }
}
