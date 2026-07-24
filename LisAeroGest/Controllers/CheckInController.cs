using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pelo processo de Check-in (Online pelo passageiro e Presencial por funcionários).
    /// </summary>
    public class CheckInController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IBoardingPassRepository _boardingPassRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa o CheckInController com as dependências necessárias.
        /// </summary>
        public CheckInController(
            ITicketRepository ticketRepository,
            ISeatRepository seatRepository,
            IBoardingPassRepository boardingPassRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            IConverterHelper converterHelper)
        {
            _ticketRepository = ticketRepository;
            _seatRepository = seatRepository;
            _boardingPassRepository = boardingPassRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Obtém o registo do Passageiro associado ao utilizador atualmente autenticado.
        /// </summary>
        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (User.Identity?.Name == null) return null;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return null;
            return await _passengerRepository.GetByUserIdAsync(user.Id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHECK-IN ONLINE (Passageiro Autenticado)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta os bilhetes pagos do passageiro que estão elegíveis para Check-in.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
            {
                TempData["Error"] = "Perfil de passageiro não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);
            var activeTickets = tickets.Where(t => t.Status == "Paid").ToList();

            return View(activeTickets);
        }

        /// <summary>
        /// Exibe o mapa de lugares disponíveis para a seleção do lugar no voo.
        /// </summary>
        /// <param name="ticketId">ID do bilhete.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SelectSeat(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var ticket = await _ticketRepository.GetWithDetailsAsync(ticketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                TempData["Error"] = "Bilhete não encontrado ou sem permissão de acesso.";
                return RedirectToAction(nameof(Index));
            }

            if (ticket.Status == "CheckedIn")
            {
                TempData["Info"] = "Já efetuou o check-in para este bilhete.";
                return RedirectToAction(nameof(Confirmation), new { ticketId = ticket.Id });
            }

            var availableSeats = await _seatRepository.GetAvailableByFlightAsync(ticket.FlightId);
            ViewBag.Ticket = ticket;
            return View(availableSeats);
        }

        /// <summary>
        /// Processa a seleção de lugar e conclui o Check-in Online gerando o Cartão de Embarque.
        /// </summary>
        /// <param name="ticketId">ID do bilhete.</param>
        /// <param name="seatId">ID do lugar escolhido.</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PerformCheckIn(int ticketId, int seatId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var ticket = await _ticketRepository.GetWithDetailsAsync(ticketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                TempData["Error"] = "Operação inválida.";
                return RedirectToAction(nameof(Index));
            }

            var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat == null || !seat.IsAvailable)
            {
                TempData["Error"] = "O lugar selecionado já não está disponível.";
                return RedirectToAction(nameof(SelectSeat), new { ticketId });
            }

            ticket.SeatId = seatId;
            ticket.Status = "CheckedIn";
            seat.IsAvailable = false;

            await _seatRepository.UpdateAsync(seat);
            await _ticketRepository.UpdateAsync(ticket);

            // Geração via ConverterHelper
            var nextSequence = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId);
            var boardingPass = _converterHelper.ToBoardingPass(ticket, nextSequence, prefix: "BOARDING");

            await _boardingPassRepository.AddAsync(boardingPass);
            await _boardingPassRepository.SaveAsync();

            TempData["Success"] = "Check-in efetuado com sucesso!";
            return RedirectToAction(nameof(Confirmation), new { ticketId = ticket.Id });
        }

        /// <summary>
        /// Executa o Check-in Direto sem seleção prévia de novo lugar (muda estado e emite cartão).
        /// </summary>
        /// <param name="ticketId">ID do bilhete.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DirectCheckIn(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var ticket = await _ticketRepository.GetWithDetailsAsync(ticketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                TempData["Error"] = "Bilhete não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (ticket.Status == "CheckedIn")
            {
                return RedirectToAction(nameof(Confirmation), new { ticketId = ticket.Id });
            }

            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            if (ticket.Seat != null)
            {
                ticket.Seat.IsAvailable = false;
                await _seatRepository.UpdateAsync(ticket.Seat);
            }

            var boardingPass = await _boardingPassRepository.GetByTicketIdAsync(ticket.Id);
            if (boardingPass == null)
            {
                // Geração via ConverterHelper
                var nextSequence = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId);
                boardingPass = _converterHelper.ToBoardingPass(ticket, nextSequence, prefix: "BOARDING");

                await _boardingPassRepository.AddAsync(boardingPass);
                await _boardingPassRepository.SaveAsync();
            }

            TempData["Success"] = "Check-in efetuado com sucesso!";
            return RedirectToAction(nameof(Confirmation), new { ticketId = ticket.Id });
        }

        /// <summary>
        /// Exibe o Cartão de Embarque (Boarding Pass) confirmado de um bilhete.
        /// </summary>
        /// <param name="ticketId">ID do bilhete.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Confirmation(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var boardingPass = await _boardingPassRepository.GetByTicketIdAsync(ticketId);

            if (boardingPass == null || boardingPass.Ticket?.PassengerId != passenger.Id)
            {
                TempData["Error"] = "Cartão de embarque não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(boardingPass);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHECK-IN PRESENCIAL / BALCÃO (Funcionários / Employee / Admin)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Exibe o painel de pesquisa do Balcão de Check-in para funcionários.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Employee,Admin")]
        public IActionResult Desk()
        {
            return View();
        }

        /// <summary>
        /// Procura bilhetes elegíveis para check-in por número de bilhete, e-mail ou documento.
        /// </summary>
        /// <param name="searchCriteria">Critério de pesquisa.</param>
        [HttpPost]
        [Authorize(Roles = "Employee,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeskSearch(string searchCriteria)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria))
            {
                ModelState.AddModelError("", "Insira um número de bilhete, e-mail ou documento.");
                return View("Desk");
            }

            var tickets = await _ticketRepository.SearchForCheckInAsync(searchCriteria);
            ViewBag.SearchCriteria = searchCriteria;
            return View("DeskResults", tickets);
        }

        /// <summary>
        /// Processa o Check-in presencial efetuado por um funcionário, definindo/atualizando a porta de embarque.
        /// </summary>
        /// <param name="ticketId">ID do bilhete.</param>
        /// <param name="gate">Porta de embarque atribuída.</param>
        [HttpPost]
        [Authorize(Roles = "Employee,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeskCheckIn(int ticketId, string? gate)
        {
            var ticket = await _ticketRepository.GetWithDetailsAsync(ticketId);
            if (ticket == null) return NotFound();

            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            var existingBoardingPass = await _boardingPassRepository.GetByTicketIdAsync(ticketId);

            if (existingBoardingPass == null)
            {
                // Geração via ConverterHelper
                var nextSequence = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId);
                var boardingPass = _converterHelper.ToBoardingPass(ticket, nextSequence, gate, prefix: "DESK");

                await _boardingPassRepository.AddAsync(boardingPass);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(gate))
                {
                    existingBoardingPass.Gate = gate;
                    await _boardingPassRepository.UpdateAsync(existingBoardingPass);
                }
            }

            await _boardingPassRepository.SaveAsync();

            TempData["Success"] = $"Check-in presencial concluído para {ticket.Passenger?.User?.FullName ?? "o passageiro"}!";
            return RedirectToAction(nameof(Desk));
        }
    }
}