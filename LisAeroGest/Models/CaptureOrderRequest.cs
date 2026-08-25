namespace LisAeroGest.Models
{
    public class CaptureOrderRequest
    {
        public int TicketId { get; set; }
        public string OrderId { get; set; } = string.Empty;
    }
}
