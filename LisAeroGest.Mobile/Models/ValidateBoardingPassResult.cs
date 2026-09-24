namespace LisAeroGest.Mobile.Models
{
    public class ValidateBoardingPassResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? ErrorMessage { get; set; }

        public int TicketId { get; set; }

        public string? FlightNumber { get; set; }

        public string? Gate { get; set; }

        public string? Status { get; set; }
    }
}