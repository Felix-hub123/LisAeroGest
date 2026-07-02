using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LisAeroGest.Controllers
{

    /// <summary>
    /// Controller responsável pela página inicial e painel de voos.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;

        public HomeController(ILogger<HomeController> logger, IFlightRepository flightRepository,
            IAirportRepository airportRepository)
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
        }

        /// <summary>
        /// Página inicial — mostra o painel de partidas e chegadas do dia.
        /// </summary>
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
                DisruptedFlightsCount = departures.Count(f => f.Status == "Delayed" || f.Status == "Cancelled")
            };

            return View(model);
        }

        /// <summary>
        /// Página de privacidade.
        /// </summary>
        [HttpGet]
        public IActionResult Privacy()
            => View();

        /// <summary>
        /// Página de erro — mostra o ID do pedido que gerou o erro.
        /// </summary>
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
