namespace LisAeroGest.Mobile.Models;

public class PayPalOrderResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ApprovalUrl { get; set; } = string.Empty;
    public string? Message { get; set; }
}
