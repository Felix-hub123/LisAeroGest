namespace LisAeroGest.Mobile.Models
{
    public class MbWayPaymentRequestDto
    {
        public int TicketId { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;
    }
}