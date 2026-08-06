using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pelo ciclo de vida, reservas e emissão de documentos dos bilhetes de voo.
    /// Gere a integração entre o repositório de dados, seleções de lugares, gestão de carrinho de compras e geração de PDF.
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
        private readonly PdfService _pdfService;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="ShopController"/>.
        /// </summary>
        /// <param name="flightRepository">Repositório para operações com voos.</param>
        /// <param name="airportRepository">Repositório para consulta de aeroportos.</param>
        /// <param name="seatRepository">Repositório para gestão e estado dos lugares na aeronave.</param>
        /// <param name="ticketRepository">Repositório para persistência de bilhetes de voo.</param>
        /// <param name="passengerRepository">Repositório para gestão dos perfis de passageiro.</param>
        /// <param name="userHelper">Helper para gestão e recuperação de utilizadores autenticados.</param>
        /// <param name="converterHelper">Helper para suporte a conversões de objetos e dropdowns.</param>
        /// <param name="pdfService">Serviço especializado na geração de documentos PDF.</param>
        public ShopController(
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            ISeatRepository seatRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            IConverterHelper converterHelper,
            PdfService pdfService)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _seatRepository = seatRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Obtém a entidade do passageiro associada ao utilizador atualmente autenticado no sistema.
        /// </summary>
        /// <returns>A instância de <see cref="Passenger"/> correspondente ou null caso não seja encontrada.</returns>
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

        /// <summary>
        /// Exibe o catálogo de voos disponíveis com base nos filtros de origem, destino e data informados.
        /// </summary>
        /// <param name="origin">Código ou identificador da origem do voo.</param>
        /// <param name="destination">Código ou identificador do destino do voo.</param>
        /// <param name="date">Data pretendida para a viagem.</param>
        /// <returns>A View contendo a lista de voos filtrados.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string origin, string destination, DateTime? date)
        {
            var flights = await _flightRepository.GetAvailableFlightsAsync(origin, destination, date);
            var airports = await _airportRepository.GetAllAsync();

            ViewBag.Airports = _converterHelper.ToAirportSelectList(airports);
            return View(flights);
        }

        /// <summary>
        /// Carrega o mapa de lugares disponíveis e ocupados da aeronave associada a um voo específico.
        /// </summary>
        /// <param name="flightId">Identificador único do voo.</param>
        /// <returns>A View de seleção do lugar com os dados da aeronave ou erro 404 caso o voo não exista.</returns>
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

        /// <summary>
        /// Adiciona uma reserva temporária de lugar ao carrinho do passageiro com validade de 15 minutos.
        /// </summary>
        /// <param name="flightId">Identificador único do voo.</param>
        /// <param name="seatId">Identificador do lugar escolhido.</param>
        /// <param name="extraLuggage">Indica se o passageiro optou por bagagem adicional.</param>
        /// <param name="mealIncluded">Indica se o passageiro optou por refeição a bordo.</param>
        /// <returns>Redirecionamento para o carrinho ou aviso caso o lugar já não esteja disponível.</returns>
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

            seat.IsAvailable = false;
            await _seatRepository.UpdateAsync(seat);

            var ticket = new Ticket
            {
                PassengerId = passenger.Id,
                FlightId = flightId,
                SeatId = seatId,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
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

        /// <summary>
        /// Exibe o conteúdo atual do carrinho do passageiro e liberta automaticamente reservas expiradas.
        /// </summary>
        /// <returns>A View do carrinho com as reservas ativas do passageiro.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved")
                .ToList();

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

        /// <summary>
        /// Remove um item específico do carrinho e restaura a disponibilidade do lugar associado.
        /// </summary>
        /// <param name="ticketId">Identificador do bilhete em reserva.</param>
        /// <returns>Redirecionamento para a vista atualizada do carrinho.</returns>
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
        // CHECKOUT, HISTÓRICO E DOWNLOAD
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finaliza a compra das reservas ativas no carrinho, alterando o estado dos bilhetes para pagos.
        /// </summary>
        /// <returns>Redirecionamento para a vista de bilhetes comprados ("MyTickets").</returns>
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

        /// <summary>
        /// Exibe o histórico de bilhetes adquiridos e em estado de check-in do passageiro autenticado.
        /// </summary>
        /// <returns>A View com a listagem dos bilhetes válidos.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Paid" || t.Status == "CheckedIn")
                .ToList();

            return View(tickets);
        }

        /// <summary>
        /// Gera e disponibiliza para transferência o ficheiro PDF do bilhete eletrónico com QR Code incorporado.
        /// </summary>
        /// <param name="ticketId">Identificador do bilhete a ser exportado.</param>
        /// <returns>Ficheiro PDF para transferência ou 404 caso o bilhete não exista/não pertença ao utilizador.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadTicketPdf(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                return NotFound();
            }

            var pdfBytes = _pdfService.GenerateTicketPdf(ticket);
            return File(pdfBytes, "application/pdf", $"Bilhete_{ticket.Id}.pdf");
        }
    }
}