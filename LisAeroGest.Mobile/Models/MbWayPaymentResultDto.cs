namespace LisAeroGest.Mobile.Models
{
    public class MbWayPaymentResultDto
    {
        public bool Success { get; set; }

        public int TicketId { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public decimal Amount { get; set; }

        public string PhoneNumber { get; set; } =
            string.Empty;

        public string TransactionId { get; set; } =
            string.Empty;

        public DateTime PaidAt { get; set; }

        public string Message { get; set; } =
            string.Empty;
    }
}