using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LisAeroGest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly WeatherService _weatherService;

        public HomeController(
            ILogger<HomeController> logger,
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            WeatherService weatherService)
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departures = await _flightRepository.GetDepartureBoardAsync();
            var arrivals = await _flightRepository.GetArrivalBoardAsync();

            var model = new HomeBoardViewModel
            {
                Departures = departures,
                Arrivals = arrivals,
                ActiveFlightsCount = departures.Count(f => f.Status == "Departed" || f.Status == "Boarding"),
                DisruptedFlightsCount = departures.Count(f => f.Status == "Delayed" || f.Status == "Cancelled"),
                Weather = await _weatherService.GetWeatherAsync("Lisbon"),
                Announcements = new()
                {
                    new() { Title = "Terminal 1 em obras", Message = "O acesso ao Terminal 1 está condicionado. Siga as placas de desvio.", Icon = "bi-cone-striped", Color = "text-warning" },
                    new() { Title = "Fast Track disponível", Message = "Passageiros Priority e Business têm acesso ao Fast Track no piso 0.", Icon = "bi-lightning-fill", Color = "text-success" },
                    new() { Title = "Estacionamento P2 lotado", Message = "Recomendamos o uso do parque P3 (shuttle gratuito a cada 5 min).", Icon = "bi-p-square-fill", Color = "text-danger" },
                }
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Privacy()
            => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
