using LisAeroGest.Data.Entities;

namespace LisAeroGest.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetWeatherAsync(string city);
    }
}