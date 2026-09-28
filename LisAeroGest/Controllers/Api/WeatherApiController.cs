using LisAeroGest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    [Route("api/weather")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WeatherApiController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherApiController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        // GET: api/weather/Lisboa
        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest(new
                {
                    message = "A cidade é obrigatória."
                });
            }

            var weather = await _weatherService.GetWeatherAsync(city);

            if (weather == null)
            {
                return NotFound(new
                {
                    message = $"Não foi possível obter a meteorologia para {city}."
                });
            }

            var windKmh = weather.Wind?.Speed * 3.6 ?? 0;
            var visibilityKm = weather.Visibility / 1000.0;

            return Ok(new
            {
                city = weather.Name ?? city,
                temperature = weather.Main?.Temp ?? 0,
                feelsLike = weather.Main?.FeelsLike ?? 0,
                humidity = weather.Main?.Humidity ?? 0,
                description =
                    weather.Weather?.FirstOrDefault()?.Description
                    ?? "Sem informação",
                icon =
                    weather.Weather?.FirstOrDefault()?.Icon
                    ?? string.Empty,
                windSpeed = Math.Round(windKmh, 1),
                visibility = Math.Round(visibilityKm, 1)
            });
        }
    }
}