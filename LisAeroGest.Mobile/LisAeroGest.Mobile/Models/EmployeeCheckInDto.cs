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

public class EmployeeCheckInRequestDto
{
    public int TicketId { get; set; }
}

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

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string Icon { get; set; } = "bi-bell";
    public string ColorClass { get; set; } = "text-primary";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = "Info";
}
