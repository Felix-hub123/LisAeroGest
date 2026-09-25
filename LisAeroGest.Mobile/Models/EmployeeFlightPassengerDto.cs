namespace LisAeroGest.Mobile.Models;

public class EmployeeFlightPassengerDto
{
    public int TicketId { get; set; }

    public string PassengerName { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public string Seat { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool CheckedIn { get; set; }

    public bool CanCheckIn =>
        !CheckedIn &&
        string.Equals(
            Status,
            "Paid",
            StringComparison.OrdinalIgnoreCase);

    public string CheckInButtonText =>
        CheckedIn
            ? "✓ Concluído"
            : "Fazer check-in";
}