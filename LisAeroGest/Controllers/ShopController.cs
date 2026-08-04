using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    public class ShopController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        public ShopController(
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            ISeatRepository seatRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            IConverterHelper converterHelper)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _seatRepository = seatRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }

        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (User.Identity?.Name == null) return null;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return null;
            return await _passengerRepository.GetByUserIdAsync(user.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PESQUISA E SELEÇÃO
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Index(string origin, string destination, DateTime? date)
        {
            var flights = await _flightRepository.GetAvailableFlightsAsync(origin, destination, date);
            var airports = await _airportRepository.GetAllAsync();

            ViewBag.Airports = _converterHelper.ToAirportSelectList(airports);
            return View(flights);
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int flightId)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null) return NotFound();

            var reservedSeatIds = flight.Aircraft?.Seats?
                .Where(s => !s.IsAvailable)
                .Select(s => s.Id)
                .ToList() ?? new List<int>();

            ViewBag.ReservedSeatIds = reservedSeatIds;
            return View(flight);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CARRINHO (RESERVAS COM STATUS "Reserved")
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int flightId, int seatId, bool extraLuggage, bool mealIncluded)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat == null || !seat.IsAvailable)
            {
                TempData["Error"] = "O lugar selecionado já não está disponível.";
                return RedirectToAction(nameof(SelectSeat), new { flightId });
            }

            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null) return NotFound();

            // Bloqueia o lugar na aeronave
            seat.IsAvailable = false;
            await _seatRepository.UpdateAsync(seat);

            // Cria o Ticket em estado de Reserva (15 minutos)
            var ticket = new Ticket
            {
                PassengerId = passenger.Id,
                FlightId = flightId,
                SeatId = seatId,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
                // Mudar de flight.Price para flight.BasePrice
                TotalPrice = flight.BasePrice + (extraLuggage ? 25 : 0) + (mealIncluded ? 15 : 0),
                Status = "Reserved",
                PurchaseDate = DateTime.UtcNow,
                ReservationExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedByUserId = passenger.UserId
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveAsync();

            TempData["Success"] = "Lugar reservado temporariamente por 15 minutos!";
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            // Procura tickets em estado "Reserved"
            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved")
                .ToList();

            // Liberta reservas expiradas
            var expiredTickets = tickets.Where(t => !t.IsReservationValid).ToList();
            if (expiredTickets.Any())
            {
                foreach (var exp in expiredTickets)
                {
                    var seat = await _seatRepository.GetByIdAsync(exp.SeatId);
                    if (seat != null)
                    {
                        seat.IsAvailable = true;
                        await _seatRepository.UpdateAsync(seat);
                    }
                    exp.Status = "Expired";
                    await _ticketRepository.UpdateAsync(exp);
                    tickets.Remove(exp);
                }
                await _ticketRepository.SaveAsync();
                TempData["Error"] = "Alguns itens no carrinho expiraram e foram libertados.";
            }

            return View(tickets);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket != null && ticket.Status == "Reserved")
            {
                var seat = await _seatRepository.GetByIdAsync(ticket.SeatId);
                if (seat != null)
                {
                    seat.IsAvailable = true;
                    await _seatRepository.UpdateAsync(seat);
                }

                ticket.Status = "Cancelled";
                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveAsync();
            }
            return RedirectToAction(nameof(Cart));
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHECKOUT E HISTÓRICO
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var reservedTickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.IsReservationValid)
                .ToList();

            if (!reservedTickets.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio ou as reservas expiraram.";
                return RedirectToAction(nameof(Cart));
            }

            // Confirma o pagamento e limpa a expiração
            foreach (var ticket in reservedTickets)
            {
                ticket.Status = "Paid";
                ticket.ReservationExpiresAt = null;
                ticket.PurchaseDate = DateTime.UtcNow;
                await _ticketRepository.UpdateAsync(ticket);
            }

            await _ticketRepository.SaveAsync();
            TempData["Success"] = "Compra efetuada com sucesso! Os seus bilhetes estão disponíveis abaixo.";
            return RedirectToAction(nameof(MyTickets));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            // Exibe bilhetes válidos comprados ou checked-in
            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Paid" || t.Status == "CheckedIn")
                .ToList();

            return View(tickets);
        }
    }
}