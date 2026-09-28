using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;
        private readonly INotificationRepository _notificationRepository;
        private readonly IWeatherService _weatherService;

        public DashboardController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            INotificationRepository notificationRepository,
            IWeatherService weatherService)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _notificationRepository = notificationRepository;
            _weatherService = weatherService;
        }




        /// <summary>
        /// Entrada única: redireciona para o dashboard da role autenticada.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return await AdminDashboard();

            if (User.IsInRole("Employee"))
                return await EmployeeDashboard();

            if (User.IsInRole("Passenger"))
                return await PassengerDashboard();

            return RedirectToAction("Index", "Home");
        }

        // =========================================================
        //  ADMIN
        // =========================================================

        /// <summary>
        /// Dashboard de supervisão e gestão.
        /// Não inclui ações operacionais de balcão.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var flights = (await _flightRepository.GetAllWithDetailsAsync()).ToList();
            var tickets = _ticketRepository.GetAllQueryable()
                .Where(t => !t.WasDeleted)
                .ToList();

            var activeStatuses = new[] { "Scheduled", "CheckIn", "Boarding" };

            var model = new AdminDashboardViewModel
            {
                TotalFlights = flights.Count(f => !f.WasDeleted),
                ActiveFlights = flights.Count(f => !f.WasDeleted && activeStatuses.Contains(f.Status)),
                DelayedFlights = flights.Count(f => !f.WasDeleted && f.Status == "Delayed"),
                CancelledFlights = flights.Count(f => !f.WasDeleted && f.Status == "Cancelled"),
                TotalTickets = tickets.Count,
                TotalRevenue = tickets
                    .Where(t => t.Status == "Paid" || t.Status == "CheckedIn")
                    .Sum(t => t.TotalPrice),
                LastUpdated = DateTime.Now
            };

            // Taxa de ocupação
            int totalSeats = 0;
            int occupiedSeats = 0;
            foreach (var f in flights.Where(f => !f.WasDeleted))
            {
                int seats = f.Seats?.Count ?? 0;
                totalSeats += seats;
                occupiedSeats += tickets.Count(t => t.FlightId == f.Id
                    && (t.Status == "Paid" || t.Status == "CheckedIn"));
            }
            model.TotalSeats = totalSeats;
            model.OccupiedSeats = occupiedSeats;
            model.OccupancyRate = totalSeats > 0
                ? Math.Round((double)occupiedSeats * 100.0 / totalSeats, 1)
                : 0;

            // Gráfico: Voos por companhia
            model.FlightsByAirline = flights
                .Where(f => !f.WasDeleted)
                .GroupBy(f => f.Airline?.Name ?? "Sem companhia")
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Gráfico: Voos por estado (labels em português)
            model.FlightsByStatus = flights
                .Where(f => !f.WasDeleted)
                .GroupBy(f => f.Status)
                .Select(g => new ChartItemViewModel
                {
                    Label = FlightStatusHelper.GetStatusText(g.Key),
                    Count = g.Count()
                })
                .ToList();

            // Gráfico: Receita por mês (últimos 12 meses)
            model.RevenueByMonth = tickets
                .Where(t => t.PurchaseDate >= DateTime.UtcNow.AddMonths(-12)
                    && (t.Status == "Paid" || t.Status == "CheckedIn"))
                .GroupBy(t => new { t.PurchaseDate.Year, t.PurchaseDate.Month })
                .Select(g => new RevenueMonthViewModel
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Total = g.Sum(t => t.TotalPrice)
                })
                .OrderBy(x => x.Label)
                .ToList();

            // Top 5 rotas
            model.TopRoutes = flights
                .Where(f => !f.WasDeleted && f.OriginAirport != null && f.DestinationAirport != null)
                .GroupBy(f => $"{f.OriginAirport!.IATACode}-{f.DestinationAirport!.IATACode}")
                .Select(g => new RouteStatViewModel
                {
                    Route = $"{g.First().OriginAirport!.IATACode} → {g.First().DestinationAirport!.IATACode}",
                    Origin = g.First().OriginAirport!.City ?? "",
                    Destination = g.First().DestinationAirport!.City ?? "",
                    FlightCount = g.Count()
                })
                .OrderByDescending(x => x.FlightCount)
                .Take(5)
                .ToList();

            // Receita por companhia (mapa a partir dos voos já carregados)
            var flightAirlineMap = flights
                .Where(f => !f.WasDeleted && f.Airline != null)
                .ToDictionary(f => f.Id, f => f.Airline!.Name ?? "Sem companhia");

            model.RevenueByAirline = tickets
                .Where(t => (t.Status == "Paid" || t.Status == "CheckedIn")
                    && flightAirlineMap.ContainsKey(t.FlightId))
                .GroupBy(t => flightAirlineMap[t.FlightId])
                .Select(g => new AirlineRevenueViewModel
                {
                    Airline = g.Key,
                    Tickets = g.Count(),
                    Revenue = g.Sum(t => t.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Alertas administrativos
            if (model.DelayedFlights > 0)
            {
                model.Alerts.Add(new DashboardAlertViewModel
                {
                    Title = $"{model.DelayedFlights} voo(s) atrasado(s)",
                    Message = "Verifique os horários previstos e as notificações enviadas.",
                    Severity = "warning",
                    Icon = "bi-clock-history",
                    Link = Url.Action("Index", "Flights")
                });
            }

            if (model.CancelledFlights > 0)
            {
                model.Alerts.Add(new DashboardAlertViewModel
                {
                    Title = $"{model.CancelledFlights} voo(s) cancelado(s)",
                    Message = "Confirme o impacto nos passageiros e nos restantes voos.",
                    Severity = "danger",
                    Icon = "bi-x-circle",
                    Link = Url.Action("Index", "Flights")
                });
            }

            var user = await GetAuthenticatedUserAsync();
            model.AdminName = user != null
                ? $"{user.FirstName} {user.LastName}".Trim()
                : User.Identity?.Name ?? "Administrador";

            return View("AdminDashboard", model);
        }

        // =========================================================
        //  EMPLOYEE
        // =========================================================

        /// <summary>
        /// Dashboard operacional do dia.
        /// Focado em voos de hoje, check-ins e alertas práticos.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> EmployeeDashboard()
        {
            var flights = (await _flightRepository.GetAllWithDetailsAsync()).ToList();
            var tickets = _ticketRepository.GetAllQueryable()
                .Where(t => !t.WasDeleted)
                .ToList();

            var today = DateTime.UtcNow.Date;
            var nextHour = DateTime.UtcNow.AddHours(1);

            var todayFlights = flights
                .Where(f => !f.WasDeleted && f.DepartureTime.Date == today)
                .OrderBy(f => f.DepartureTime)
                .ToList();

            var activeStatuses = new[] { "Scheduled", "CheckIn", "Boarding" };
            var todayFlightIds = todayFlights.Select(f => f.Id).ToHashSet();

            var pendingCheckIns = tickets.Count(t =>
                todayFlightIds.Contains(t.FlightId)
                && t.Status == "Paid");

            var model = new EmployeeDashboardViewModel
            {
                TodayFlights = todayFlights.Count,
                UpcomingFlights = todayFlights.Count(f =>
                    f.DepartureTime <= nextHour
                    && f.DepartureTime >= DateTime.UtcNow
                    && f.Status != "Cancelled"
                    && f.Status != "Departed"),
                DelayedToday = todayFlights.Count(f => f.Status == "Delayed"),
                CancelledToday = todayFlights.Count(f => f.Status == "Cancelled"),
                PendingCheckIns = pendingCheckIns,
                BoardingNow = todayFlights.Count(f => f.Status == "Boarding"),
                ActiveFlights = flights.Count(f => !f.WasDeleted && activeStatuses.Contains(f.Status)),
                LastUpdated = DateTime.Now
            };

            // Gráfico de estados
            model.FlightsByStatus = flights
                .Where(f => !f.WasDeleted)
                .GroupBy(f => f.Status)
                .Select(g => new ChartItemViewModel
                {
                    Label = FlightStatusHelper.GetStatusText(g.Key),
                    Count = g.Count()
                })
                .ToList();

            // Lista de voos de hoje
            model.TodayFlightList = todayFlights.Select(f =>
            {
                var flightTickets = tickets.Where(t => t.FlightId == f.Id).ToList();
                return new TodayFlightItemViewModel
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber ?? "",
                    AirlineName = f.Airline?.Name ?? "—",
                    OriginCode = f.OriginAirport?.IATACode ?? "—",
                    DestinationCode = f.DestinationAirport?.IATACode ?? "—",
                    DepartureTime = f.DepartureTime,
                    DelayedDepartureTime = f.DelayedDepartureTime,
                    GateNumber = f.Gate?.GateNumber,
                    Status = f.Status,
                    PassengerCount = flightTickets.Count(t => t.Status == "Paid" || t.Status == "CheckedIn"),
                    PendingCheckInCount = flightTickets.Count(t => t.Status == "Paid")
                };
            }).ToList();

            // Notificações
            var user = await GetAuthenticatedUserAsync();
            if (user != null)
            {
                var notifications = (await _notificationRepository.GetByUserAsync(user.Id)).ToList();
                model.RecentNotifications = notifications.Take(5).ToList();
                model.UnreadCount = notifications.Count(n => !n.IsRead);
                model.EmployeeName = $"{user.FirstName} {user.LastName}".Trim();
            }
            else
            {
                model.EmployeeName = User.Identity?.Name ?? "Funcionário";
            }

            return View("EmployeeDashboard", model);
        }

        // =========================================================
        //  PASSENGER
        // =========================================================

        /// <summary>
        /// Dashboard pessoal do passageiro.
        /// Mostra apenas as suas viagens e ações relacionadas.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> PassengerDashboard()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
                return RedirectToAction("Index", "Home");

            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id)).ToList();

            // ── Próximos voos (pagos, ainda não partiram) ──
            var upcoming = tickets
                .Where(t => t.Flight != null
                    && t.Flight.DepartureTime > DateTime.UtcNow
                    && (t.Status == "Paid" || t.Status == "CheckedIn"))
                .OrderBy(t => t.Flight!.DepartureTime)
                .ToList();

            // ── Reservas pendentes (aguardam pagamento, ainda válidas) ──
            var pending = tickets
                .Where(t => t.Status == "Reserved" && t.IsReservationValid)
                .OrderBy(t => t.ReservationExpiresAt)
                .ToList();

            // ── Histórico (já partiram, ou foram cancelados/expirados) ──
            var past = tickets
                .Where(t => t.Flight != null
                    && (t.Flight.DepartureTime <= DateTime.UtcNow
                        || t.Status == "Cancelled"
                        || t.Status == "Expired"))
                .OrderByDescending(t => t.Flight!.DepartureTime)
                .ToList();

            // ── Total gasto (só bilhetes pagos) ──
            var totalSpent = tickets
                .Where(t => t.Status == "Paid" || t.Status == "CheckedIn")
                .Sum(t => t.TotalPrice);

            // ── Notificações ──
            var allNotifications = (await _notificationRepository.GetByUserAsync(user.Id)).ToList();
            var recentNotifications = allNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(4)
                .ToList();
            var unreadCount = allNotifications.Count(n => !n.IsRead);

            // ── LisAeroPoints ──
            // Regra: 1€ = 10 pontos
            var loyaltyPoints = (int)(totalSpent * 10);

            // Níveis (regra)
            string tier;
            int pointsToNext;
            int progressPercent;
            int nextThreshold;

            if (loyaltyPoints < 500)
            {
                tier = "Bronze";
                nextThreshold = 500;
            }
            else if (loyaltyPoints < 1500)
            {
                tier = "Prata";
                nextThreshold = 1500;
            }
            else if (loyaltyPoints < 3000)
            {
                tier = "Ouro";
                nextThreshold = 3000;
            }
            else
            {
                tier = "Platina";
                nextThreshold = loyaltyPoints; // já no topo
            }

            pointsToNext = Math.Max(0, nextThreshold - loyaltyPoints);
            progressPercent = nextThreshold > 0
                ? Math.Min(100, (int)Math.Round((double)loyaltyPoints * 100 / nextThreshold))
                : 100;

            // ── Mapper ──
            PassengerTicketItemViewModel MapTicket(Ticket t)
            {
                var flight = t.Flight!;
                var departure = flight.DelayedDepartureTime ?? flight.DepartureTime;
                var hoursUntil = (departure - DateTime.UtcNow).TotalHours;

                bool canCheckIn = t.Status == "Paid"
                    && hoursUntil <= 48
                    && hoursUntil >= 1
                    && flight.Status != "Cancelled"
                    && flight.Status != "Departed";

                return new PassengerTicketItemViewModel
                {
                    TicketId = t.Id,
                    FlightNumber = flight.FlightNumber ?? "",
                    AirlineName = flight.Airline?.Name ?? "—",
                    OriginCode = flight.OriginAirport?.IATACode ?? "—",
                    DestinationCode = flight.DestinationAirport?.IATACode ?? "—",
                    OriginCity = flight.OriginAirport?.City ?? "",
                    DestinationCity = flight.DestinationAirport?.City ?? "",
                    DepartureTime = flight.DepartureTime,
                    DelayedDepartureTime = flight.DelayedDepartureTime,
                    GateNumber = flight.Gate?.GateNumber,
                    FlightStatus = flight.Status,
                    TicketStatus = t.Status,
                    SeatNumber = t.Seat?.Code,
                    CanCheckIn = canCheckIn,
                    HasBoardingPass = t.Status == "CheckedIn"
                };
            }

            var model = new PassengerDashboardViewModel
            {
                Passenger = passenger,
                FullName = $"{passenger.FirstName} {passenger.LastName}".Trim(),

                UpcomingTickets = upcoming.Select(MapTicket).ToList(),
                PendingReservations = pending.Select(MapTicket).ToList(),
                PastTickets = past.Select(MapTicket).ToList(),

                UpcomingCount = upcoming.Count,
                PendingCount = pending.Count,
                PastCount = past.Count,

                TotalSpent = totalSpent,

                RecentNotifications = recentNotifications,
                UnreadNotificationsCount = unreadCount,

                LoyaltyPoints = loyaltyPoints,
                LoyaltyTier = tier,
                PointsToNextTier = pointsToNext,
                LoyaltyProgressPercent = progressPercent
            };

            model.NextFlight = model.UpcomingTickets.FirstOrDefault();

            return View("PassengerDashboard", model);
        }

        // =========================================================
        //  Helpers
        // =========================================================

        /// <summary>
        /// Obtém o utilizador autenticado a partir do email da identidade.
        /// </summary>
        private async Task<User?> GetAuthenticatedUserAsync()
        {
            if (User.Identity?.Name == null)
                return null;

            return await _userHelper.GetUserByEmailAsync(User.Identity.Name);
        }




        // =========================================================
        //  METEOROLOGIA OPERACIONAL
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> OperationalWeather()
        {
            var flights = (await _flightRepository.GetAllWithDetailsAsync())
                .Where(f => !f.WasDeleted
                    && f.DepartureTime >= DateTime.UtcNow
                    && f.Status != "Cancelled")
                .OrderBy(f => f.DepartureTime)
                .ToList();

            var locations = new List<(string City, string AirportCode)>
    {
        ("Lisbon", "LIS")
    };

            // Adicionar destinos dos próximos voos.
            foreach (var flight in flights)
            {
                if (flight.DestinationAirport == null)
                    continue;

                var city = flight.DestinationAirport.City;
                var code = flight.DestinationAirport.IATACode;

                if (string.IsNullOrWhiteSpace(city))
                    continue;

                if (!locations.Any(x =>
                    x.City.Equals(city, StringComparison.OrdinalIgnoreCase)))
                {
                    locations.Add((city, code ?? ""));
                }
            }

            var model = new OperationalWeatherViewModel
            {
                LastUpdated = DateTime.Now
            };

            foreach (var location in locations)
            {
                var weather =
                    await _weatherService.GetWeatherAsync(location.City);

                if (weather == null)
                    continue;

                var windKmh =
                    (weather.Wind?.Speed ?? 0) * 3.6;

                var visibilityKm =
                    weather.Visibility / 1000.0;

                var description =
                    weather.Weather?.FirstOrDefault()?.Description
                    ?? "Sem informação";

                var item = new OperationalWeatherItemViewModel
                {
                    City = weather.Name ?? location.City,

                    AirportCode = location.AirportCode,

                    Temperature =
                        Math.Round(weather.Main?.Temp ?? 0, 1),

                    FeelsLike =
                        Math.Round(weather.Main?.FeelsLike ?? 0, 1),

                    Humidity =
                        weather.Main?.Humidity ?? 0,

                    Description = description,

                    Icon =
                        weather.Weather?.FirstOrDefault()?.Icon
                        ?? string.Empty,

                    WindSpeedKmh =
                        Math.Round(windKmh, 1),

                    VisibilityKm =
                        Math.Round(visibilityKm, 1)
                };

                // Alertas informativos do projeto.
                // Não alteram automaticamente o estado do voo.
                var alerts = new List<string>();

                if (item.WindSpeedKmh >= 40)
                {
                    alerts.Add("Vento forte");
                }

                if (item.VisibilityKm < 5)
                {
                    alerts.Add("Visibilidade reduzida");
                }

                var condition = description.ToLowerInvariant();

                if (condition.Contains("trovoada") ||
                    condition.Contains("thunderstorm"))
                {
                    alerts.Add("Trovoada");
                }

                if (alerts.Count > 0)
                {
                    item.HasAlert = true;
                    item.AlertMessage = string.Join(" • ", alerts);
                }

                model.Locations.Add(item);
            }

            return View(model);
        }
    }
}