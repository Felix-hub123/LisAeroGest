using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers; // ← Adicionar esta referência
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LisAeroGest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly WeatherService _weatherService;
        private readonly IConverterHelper _converterHelper; 

        public HomeController(
            ILogger<HomeController> logger,
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            WeatherService weatherService,
            IConverterHelper converterHelper) 
        {
            _logger = logger;
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _weatherService = weatherService;
            _converterHelper = converterHelper; 
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Buscar TODOS os voos
            var allDepartures = await _flightRepository.GetDepartureBoardAsync();
            var allArrivals = await _flightRepository.GetArrivalBoardAsync();

            // 🔧 FILTRAR APENAS VOOS FUTUROS E MOSTRAR OS PRÓXIMOS
            var now = DateTime.Now;

            // PARTIDAS: mostrar apenas os próximos 8 voos
            var departures = allDepartures
                .Where(f => f.DepartureTime > now)
                .OrderBy(f => f.DepartureTime)
                .Take(8)
                .ToList();

            // CHEGADAS: mostrar apenas os próximos 5 voos
            var arrivals = allArrivals
                .Where(f => f.ArrivalTime > now)
                .OrderBy(f => f.ArrivalTime)
                .Take(5)
                .ToList();

            // 🔄 CONVERTER para FlightDetailViewModel (com histórico virtual)
            var departureDetails = _converterHelper.ToFlightDetailViewModelList(departures);
            var arrivalDetails = _converterHelper.ToFlightDetailViewModelList(arrivals);

            // 🔧 DESTINOS POPULARES: usar TODOS os voos futuros (não apenas os 8)
            var destinos = allDepartures
                .Where(f => f.DestinationAirport != null && f.DepartureTime > now)
                .GroupBy(f => f.DestinationAirport!.IATACode)
                .Select(g => new PopularDestination
                {
                    IATA = g.Key,
                    Cidade = g.First().DestinationAirport!.City!,
                    Pais = g.First().DestinationAirport!.Country!,
                    Voos = g.Count()
                })
                .OrderByDescending(x => x.Voos)
                .Take(6)
                .ToList();

            // 🔧 AVISOS ATIVOS: usar TODOS os voos futuros (não apenas os 8)
            var avisos = allDepartures
                .Where(f => f.DepartureTime > now && (f.Status == "Boarding" || f.Status == "CheckIn"))
                .Take(4)
                .Select(f => new FlightWarning
                {
                    FlightNumber = f.FlightNumber ?? "N/A",
                    Origin = f.OriginAirport?.IATACode ?? "",
                    Destination = f.DestinationAirport?.IATACode ?? "",
                    Status = f.Status
                })
                .ToList();

            // 🔧 KPIs: usar TODOS os voos futuros
            var activeFlightsCount = allDepartures.Count(f => f.DepartureTime > now && (f.Status == "Departed" || f.Status == "Boarding"));
            var disruptedFlightsCount = allDepartures.Count(f => f.DepartureTime > now && (f.Status == "Delayed" || f.Status == "Cancelled"));

            var model = new HomeBoardViewModel
            {
                Departures = departures,           // ← Apenas 8 voos
                Arrivals = arrivals,               // ← Apenas 5 voos
                DepartureDetails = departureDetails,
                ArrivalDetails = arrivalDetails,
                // ⚠️ TotalPartidas e TotalChegadas são calculadas automaticamente!
                ActiveFlightsCount = activeFlightsCount,
                DisruptedFlightsCount = disruptedFlightsCount,
                PopularDestinations = destinos,
                ActiveWarnings = avisos,
                Weather = await _weatherService.GetWeatherAsync("Lisbon"),
                Announcements = new List<Announcement>
                {
                    new() { Title = "Terminal 1 em obras", Message = "O acesso ao Terminal 1 está condicionado. Siga as placas de desvio.", Icon = "bi-cone-striped", Color = "text-warning" },
                    new() { Title = "Fast Track disponível", Message = "Passageiros Priority e Business têm acesso ao Fast Track no piso 0.", Icon = "bi-lightning-fill", Color = "text-success" },
                    new() { Title = "Estacionamento P2 lotado", Message = "Recomendamos o uso do parque P3 (shuttle gratuito a cada 5 min).", Icon = "bi-p-square-fill", Color = "text-danger" },
                }
            };

            ViewData["Converter"] = _converterHelper;

            return View(model);
        }

        // 🆕 Endpoint para obter detalhes de um voo (usado pelo modal)
        [HttpGet("flight/{id}")]
        public async Task<IActionResult> GetFlightDetail(int id)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(id);

            if (flight == null)
                return NotFound(new { error = "Voo não encontrado" });

            var detail = _converterHelper.ToFlightDetailViewModel(flight);
            return Ok(detail);
        }

        // 🆕 Endpoint para pesquisa de voos (API pública)
        [HttpGet("api/public/flights/search")]
        public async Task<IActionResult> SearchFlights([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Termo de pesquisa obrigatório" });

            var flights = await _flightRepository.SearchFlightsAsync(q);
            var details = _converterHelper.ToFlightDetailViewModelList(flights);
            return Ok(details);
        }



        /// <summary>
        /// Página de Privacidade
        /// </summary>
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Privacidade - LisAeroGest";
            return View();
        }


        [HttpGet("api/public/flights/available")]
        public async Task<IActionResult> GetAvailableFlights()
        {
            var departures = await _flightRepository.GetAllQueryable()
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime.Date == DateTime.Today || f.DepartureTime.Date == DateTime.Today.AddDays(1))
                .OrderBy(f => f.DepartureTime)
                .Take(50)
                .ToListAsync();

            var details = _converterHelper.ToFlightDetailViewModelList(departures);
            return Ok(details);
        }


        /// <summary>
        /// Página de Ajuda / Perguntas Frequentes
        /// </summary>
        [AllowAnonymous]
        public IActionResult Help()
        {
            ViewData["Title"] = "Ajuda - LisAeroGest";
            return View();
        }

        /// <summary>
        /// Página de Contactos
        /// </summary>
        [AllowAnonymous]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contactos - LisAeroGest";
            return View();
        }

      

        /// <summary>
        /// Página de Termos e Condições
        /// </summary>
        [AllowAnonymous]
        public IActionResult Terms()
        {
            ViewData["Title"] = "Termos e Condições - LisAeroGest";
            return View();
        }
    }
}