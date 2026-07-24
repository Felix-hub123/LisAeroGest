using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela loja de bilhetes, carrinho e checkout.
    /// </summary>
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

        /// <summary>
        /// Obtém o registo de Passageiro associado ao utilizador autenticado.
        /// </summary>
        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (User.Identity?.Name == null) return null;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return null;
            return await _passengerRepository.GetByUserIdAsync(user.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PESQUISA E LOJA
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

            // IDs dos lugares indisponíveis para o mapa de lugares
            var reservedSeatIds = flight.Aircraft?.Seats?
                .Where(s => !s.IsAvailable)
                .Select(s => s.Id)
                .ToList() ?? new List<int>();

            ViewBag.ReservedSeatIds = reservedSeatIds;

            return View(flight);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CARRINHO E RESERVAS TEMPORÁRIAS
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

  
            seat.IsAvailable = false;
            await _seatRepository.UpdateAsync(seat);

            // Mapeamento delegado ao ConverterHelper
            var temp = _converterHelper.ToTicketTemp(flightId, seatId, passenger, extraLuggage, mealIncluded);

            await _ticketRepository.AddTempAsync(temp);
            await _ticketRepository.SaveAsync();

            TempData["Success"] = "Assento reservado temporariamente por 15 minutos!";
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var items = (await _ticketRepository.GetTempByUserAsync(passenger.Id)).ToList();

            // Limpa reservas expiradas
            var expiredItems = items.Where(i => i.ExpiresAt < DateTime.UtcNow).ToList();
            if (expiredItems.Any())
            {
                foreach (var exp in expiredItems)
                {
                    var seat = await _seatRepository.GetByIdAsync(exp.SeatId);
                    if (seat != null)
                    {
                        seat.IsAvailable = true;
                        await _seatRepository.UpdateAsync(seat);
                    }
                    await _ticketRepository.DeleteTempAsync(exp);
                    items.Remove(exp);
                }
                await _ticketRepository.SaveAsync();
                TempData["Error"] = "Alguns itens no carrinho expiraram e foram removidos.";
            }

            return View(items);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int tempId)
        {
            var temp = await _ticketRepository.GetTempByIdAsync(tempId);
            if (temp != null)
            {
                var seat = await _seatRepository.GetByIdAsync(temp.SeatId);
                if (seat != null)
                {
                    seat.IsAvailable = true;
                    await _seatRepository.UpdateAsync(seat);
                }
                await _ticketRepository.DeleteTempAsync(temp);
                await _ticketRepository.SaveAsync();
            }
            return RedirectToAction(nameof(Cart));
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHECKOUT E HISTÓRICO DE BILHETES
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var tempItems = (await _ticketRepository.GetTempByUserAsync(passenger.Id))
                .Where(i => i.ExpiresAt >= DateTime.UtcNow)
                .ToList();

            if (!tempItems.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio ou a reserva expirou.";
                return RedirectToAction(nameof(Cart));
            }

            foreach (var item in tempItems)
            {
                var flight = await _flightRepository.GetByIdAsync(item.FlightId);

                // Mapeamento e cálculo delegados ao ConverterHelper
                var ticket = _converterHelper.ToTicket(item, flight, passenger.UserId);

                await _ticketRepository.AddAsync(ticket);
                await _ticketRepository.DeleteTempAsync(item);
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

            var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);
            return View(tickets);
        }
    }
}