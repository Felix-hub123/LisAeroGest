namespace LisAeroGest.Mobile.Models;

public class PaymentCaptureResultDto
{
    public bool Success { get; set; }
    public int TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
