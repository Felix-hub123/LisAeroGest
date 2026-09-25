namespace LisAeroGest.Mobile.Models;

public class FlightCommunicationResultDto
{
    public bool Success { get; set; }

    public int Recipients { get; set; }

    public string Message { get; set; } = string.Empty;
}