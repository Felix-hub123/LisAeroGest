namespace LisAeroGest.Mobile.Models
{
    public class ChangeGateResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public int FlightId { get; set; }

        public string FlightNumber { get; set; } = string.Empty;

        public string PreviousGate { get; set; } = string.Empty;

        public int GateId { get; set; }

        public string GateNumber { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;
    }
}