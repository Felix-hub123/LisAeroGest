using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
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
        private readonly INotificationRepository _notificationRepository;

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
            IUserHelper userHelper,
            INotificationRepository notificationRepository)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Apresenta o dashboard consoante a role do utilizador autenticado.
        /// </summary>
        /// <returns>View apropriada para a role do utilizador.</returns>
        [HttpGet]
        [Authorize]
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
        /// 



        [Authorize(Roles = "Admin")]
        private async Task<IActionResult> AdminDashboard()
        {
            var flights = await _flightRepository.GetAllAsync();

            var tickets = await _ticketRepository.GetAllAsync();


            // ---- KPIs básicos ----

            ViewBag.TotalFlights = flights.Count();

            ViewBag.TotalTickets = tickets.Count(t => !t.WasDeleted);

            ViewBag.ActiveFlights = flights.Count(f => !f.WasDeleted &&

                (f.Status == "Scheduled" || f.Status == "CheckIn" || f.Status == "Boarding"));

            ViewBag.TotalRevenue = tickets

                .Where(t => !t.WasDeleted && (t.Status == "Paid" || t.Status == "CheckedIn"))

                .Sum(t => t.TotalPrice);

            ViewBag.DelayedFlights = flights.Count(f => !f.WasDeleted && f.Status == "Delayed");

            ViewBag.CancelledFlights = flights.Count(f => !f.WasDeleted && f.Status == "Cancelled");


            // ---- Taxa de ocupação média ----

            int totalSeats = 0;

            int occupiedSeats = 0;

            foreach (var f in flights.Where(f => !f.WasDeleted))

            {

                int seats = f.Seats?.Count ?? 0;

                totalSeats += seats;

                occupiedSeats += tickets.Count(t => !t.WasDeleted && t.FlightId == f.Id

                    && (t.Status == "Paid" || t.Status == "CheckedIn"));

            }

            ViewBag.OccupancyRate = totalSeats > 0

                ? Math.Round((double)occupiedSeats * 100.0 / totalSeats, 1)

                : 0;

            ViewBag.OccupiedSeats = occupiedSeats;

            ViewBag.TotalSeats = totalSeats;


            // ---- Gráfico 1: Voos por companhia ----

            var flightsByAirline = flights

                .Where(f => !f.WasDeleted)

                .GroupBy(f => f.Airline?.Name ?? "Sem companhia")

                .Select(g => new { label = g.Key, count = g.Count() })

                .OrderByDescending(x => x.count)

                .ToList();


            // ---- Gráfico 2: Voos por estado ----

            var flightsByStatus = flights

                .Where(f => !f.WasDeleted)

                .GroupBy(f => f.Status)

                .Select(g => new { label = g.Key, count = g.Count() })

                .ToList();


            // ---- Gráfico 3: Receita por mês (últimos 12 meses) ----

            var revenueByMonth = tickets

                .Where(t => !t.WasDeleted && t.PurchaseDate >= DateTime.UtcNow.AddMonths(-12)

                    && (t.Status == "Paid" || t.Status == "CheckedIn"))

                .GroupBy(t => new { t.PurchaseDate.Year, t.PurchaseDate.Month })

                .Select(g => new

                {

                    label = $"{g.Key.Year}-{g.Key.Month:D2}",

                    total = g.Sum(t => t.TotalPrice)

                })

                .OrderBy(x => x.label)

                .ToList();


            // ---- Tabela: Top 5 rotas mais populares ----

            var topRoutes = flights

                .Where(f => !f.WasDeleted && f.OriginAirport != null && f.DestinationAirport != null)

                .GroupBy(f => $"{f.OriginAirport!.IATACode}-{f.DestinationAirport!.IATACode}")

                .Select(g => new

                {

                    rota = $"{g.First().OriginAirport!.IATACode} → {g.First().DestinationAirport!.IATACode}",

                    origem = g.First().OriginAirport!.City,

                    destino = g.First().DestinationAirport!.City,

                    voos = g.Count()

                })

                .OrderByDescending(x => x.voos)

                .Take(5)

                .ToList();


            // ---- Tabela: Receita por companhia ----

            var revenueByAirline = tickets

                .Where(t => !t.WasDeleted && t.Flight?.Airline != null

                    && (t.Status == "Paid" || t.Status == "CheckedIn"))

                .GroupBy(t => t.Flight!.Airline!.Name)

                .Select(g => new

                {

                    companhia = g.Key,

                    bilhetes = g.Count(),

                    receita = g.Sum(t => t.TotalPrice)

                })

                .OrderByDescending(x => x.receita)

                .ToList();


            // Serializar para JS

            ViewBag.FlightsByAirline = System.Text.Json.JsonSerializer.Serialize(flightsByAirline);

            ViewBag.FlightsByStatus = System.Text.Json.JsonSerializer.Serialize(flightsByStatus);

            ViewBag.RevenueByMonth = System.Text.Json.JsonSerializer.Serialize(revenueByMonth);

            ViewBag.TopRoutes = System.Text.Json.JsonSerializer.Serialize(topRoutes);

            ViewBag.RevenueByAirline = System.Text.Json.JsonSerializer.Serialize(revenueByAirline);


            return View("AdminDashboard");
        }

        /// <summary>
        /// Dashboard operacional para Funcionários com foco nos voos do dia.
        /// </summary>
        /// <returns>View com indicadores operacionais.</returns>
        /// 




        [Authorize(Roles = "Employee")]
        private async Task<IActionResult> EmployeeDashboard()
        {
            var flights = await _flightRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();

            ViewBag.TotalFlights = flights.Count(f => !f.WasDeleted);
            ViewBag.TotalTickets = tickets.Count(t => !t.WasDeleted);
            ViewBag.ActiveFlights = flights.Count(f => !f.WasDeleted &&
                (f.Status == "Scheduled" || f.Status == "CheckIn" || f.Status == "Boarding"));

            var flightsByStatus = flights
                .Where(f => !f.WasDeleted)
                .GroupBy(f => f.Status)
                .Select(g => new { label = g.Key, count = g.Count() });

            ViewBag.FlightsByStatus = System.Text.Json.JsonSerializer.Serialize(flightsByStatus);

            // ---- Notificações recentes ----
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user != null)
            {
                var notifications = await _notificationRepository.GetByUserAsync(user.Id);
                ViewBag.RecentNotifications = notifications.Take(5).ToList();
                ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);
            }

            return View("EmployeeDashboard");
        }

        /// <summary>
        /// Dashboard pessoal para Passageiros com os seus bilhetes e voos futuros.
        /// </summary>
        /// <returns>View com os dados do passageiro.</returns>
        /// 


        [Authorize(Roles = "Passenger")]
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
