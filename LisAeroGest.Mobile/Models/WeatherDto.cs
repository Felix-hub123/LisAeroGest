namespace LisAeroGest.Mobile.Models;

public class WeatherDto
{
    public string City { get; set; } = string.Empty;

    public double Temperature { get; set; }

    public double FeelsLike { get; set; }

    public int Humidity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    // O Backend já devolve km/h
    public double WindSpeed { get; set; }

    // O Backend já devolve km
    public double Visibility { get; set; }
}