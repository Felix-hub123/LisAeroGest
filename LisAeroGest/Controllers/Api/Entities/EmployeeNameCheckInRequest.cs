namespace LisAeroGest.Controllers.Api.Entities
{
    public class EmployeeNameCheckInRequest
    {
        public string PassengerName { get; set; } = string.Empty;

        public string? FlightNumber { get; set; }

        public string? DocumentNumber { get; set; }
    }
}
