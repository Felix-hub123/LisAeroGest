using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pelo dashboard de estatísticas.
    /// Apresenta conteúdo diferente consoante a role do utilizador autenticado.
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;

        /// <summary>
        /// Inicializa o DashboardController com as dependências necessárias.
        /// </summary>
        /// <param name="flightRepository">Repositório de voos para estatísticas de voos.</param>
        /// <param name="ticketRepository">Repositório de bilhetes para estatísticas de vendas.</param>
        /// <param name="passengerRepository">Repositório de passageiros para dados do passageiro.</param>
        /// <param name="userHelper">Helper de utilizadores para obter o utilizador autenticado.</param>
        public DashboardController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
        }

        /// <summary>
        /// Apresenta o dashboard consoante a role do utilizador autenticado.
        /// </summary>
        /// <returns>View apropriada para a role do utilizador.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated && User.IsInRole("Admin"))
                return await AdminDashboard();

            if (User.Identity!.IsAuthenticated && User.IsInRole("Employee"))
                return await EmployeeDashboard();

            if (User.Identity!.IsAuthenticated && User.IsInRole("Passenger"))
                return await PassengerDashboard();

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Dashboard completo para Administradores com estatísticas globais.
        /// </summary>
        /// <returns>View com gráficos e indicadores do aeroporto.</returns>
        private async Task<IActionResult> AdminDashboard()
        {
            var flights = await _flightRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();

            ViewBag.TotalFlights = flights.Count();
            ViewBag.TotalTickets = tickets.Count();
            ViewBag.TotalRevenue = tickets.Where(t => t.Status == "Paid" || t.Status == "CheckedIn").Sum(t => t.TotalPrice);
            ViewBag.ActiveFlights = flights.Count(f => f.Status == "Scheduled" || f.Status == "CheckIn" || f.Status == "Boarding");

            var flightsByAirline = flights
                .GroupBy(f => f.Airline?.Name ?? "Sem companhia")
                .Select(g => new { label = g.Key, count = g.Count() });

            var flightsByStatus = flights
                .GroupBy(f => f.Status)
                .Select(g => new { label = g.Key, count = g.Count() });

            ViewBag.FlightsByAirline = System.Text.Json.JsonSerializer.Serialize(flightsByAirline);
            ViewBag.FlightsByStatus = System.Text.Json.JsonSerializer.Serialize(flightsByStatus);

            return View("AdminDashboard");
        }

        /// <summary>
        /// Dashboard operacional para Funcionários com foco nos voos do dia.
        /// </summary>
        /// <returns>View com indicadores operacionais.</returns>
        private async Task<IActionResult> EmployeeDashboard()
        {
            var flights = await _flightRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();

            ViewBag.TotalFlights = flights.Count();
            ViewBag.TotalTickets = tickets.Count();
            ViewBag.ActiveFlights = flights.Count(f => f.Status == "Scheduled" || f.Status == "CheckIn" || f.Status == "Boarding");

            var flightsByStatus = flights
                .GroupBy(f => f.Status)
                .Select(g => new { label = g.Key, count = g.Count() });

            ViewBag.FlightsByStatus = System.Text.Json.JsonSerializer.Serialize(flightsByStatus);

            return View("EmployeeDashboard");
        }

        /// <summary>
        /// Dashboard pessoal para Passageiros com os seus bilhetes e voos futuros.
        /// </summary>
        /// <returns>View com os dados do passageiro.</returns>
        private async Task<IActionResult> PassengerDashboard()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null) return RedirectToAction("Index", "Home");

            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger == null) return RedirectToAction("Index", "Home");

            var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);
            var upcomingTickets = tickets.Where(t => t.Flight!.DepartureTime > DateTime.UtcNow && t.Status != "Cancelled");
            var pastTickets = tickets.Where(t => t.Flight!.DepartureTime <= DateTime.UtcNow || t.Status == "Cancelled");

            ViewBag.Passenger = passenger;
            ViewBag.UpcomingTickets = upcomingTickets;
            ViewBag.PastTickets = pastTickets;

            return View("PassengerDashboard");
        }
    }
}
