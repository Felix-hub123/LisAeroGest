namespace LisAeroGest.Mobile.Models;

public class FlightCommunicationRequestDto
{
    public string Type { get; set; } = "Info";

    public string Message { get; set; } = string.Empty;
}