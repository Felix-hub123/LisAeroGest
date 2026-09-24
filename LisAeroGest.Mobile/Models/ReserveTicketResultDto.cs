namespace LisAeroGest.Mobile.Models;

public class ReserveTicketResultDto
{
    public int TicketId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string SeatCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
