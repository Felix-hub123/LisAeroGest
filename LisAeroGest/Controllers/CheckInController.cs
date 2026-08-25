using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão do processo de check-in e emissão dos respetivos cartões de embarque.
    /// Gere a validação dos bilhetes, atribuição de sequência de embarque e geração de PDF.
    /// </summary>
    public class CheckInController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IBoardingPassRepository _boardingPassRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;
        private readonly PdfService _pdfService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IFlightRepository _flightRepository;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="CheckInController"/>.
        /// </summary>
        /// <param name="ticketRepository">Repositório de bilhetes.</param>
        /// <param name="boardingPassRepository">Repositório para persistência de cartões de embarque.</param>
        /// <param name="passengerRepository">Repositório para dados do passageiro.</param>
        /// <param name="userHelper">Helper de gestão de utilizadores.</param>
        /// <param name="pdfService">Serviço de geração de documentos PDF.</param>
        /// <param name="qrCodeService">Serviço de geração local de QR Codes.</param>
        public CheckInController(
            ITicketRepository ticketRepository,
            IBoardingPassRepository boardingPassRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            PdfService pdfService,
            IQrCodeService qrCodeService,
            IFlightRepository flightRepository,
            IConverterHelper converterHelper)
        {
            _ticketRepository = ticketRepository;
            _boardingPassRepository = boardingPassRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _pdfService = pdfService;
            _qrCodeService = qrCodeService;
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Obtém a entidade do passageiro associada ao utilizador atualmente autenticado.
        /// </summary>
        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (User.Identity?.Name == null) return null;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return null;
            return await _passengerRepository.GetByUserIdAsync(user.Id);
        }

        /// <summary>
        /// Lista os bilhetes elegíveis para check-in do passageiro autenticado.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Paid")
                .ToList();

            return View(tickets);
        }


        /// <summary>
        /// Exibe a página do balcão de check-in presencial para funcionários.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Employee, Admin")]
        public IActionResult EmployeeCheckIn()
        {
            return View();
        }

        /// <summary>
        /// Processa a pesquisa e efetua o check-in no balcão feito pelo funcionário.
        /// </summary>
        /// <param name="searchTerm">ID do bilhete ou documento de identificação do passageiro.</param>
        [HttpPost]
        [Authorize(Roles = "Employee, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeCheckIn(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["Error"] = "Por favor, introduza o ID do bilhete ou documento de identificação.";
                return View();
            }

            var tickets = await _ticketRepository.GetAllAsync();

            bool isNumeric = int.TryParse(searchTerm, out int ticketId);

            var ticket = tickets.FirstOrDefault(t =>
                (isNumeric && t.Id == ticketId) ||
                (t.Passenger != null && t.Passenger.DocumentNumber != null && t.Passenger.DocumentNumber.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
            );

            if (ticket == null)
            {
                TempData["Error"] = "Nenhum bilhete encontrado para a pesquisa introduzida.";
                return View();
            }

            var ticketWithDetails = await _ticketRepository.GetTicketWithDetailsAsync(ticket.Id);

            if (ticketWithDetails!.HasCheckedIn)
            {
                TempData["Warning"] = "Este passageiro já efetuou o check-in.";
            }

            return View(ticketWithDetails);
        }

        /// <summary>
        /// Executa o processo de check-in para um bilhete específico e gera o cartão de embarque.
        /// </summary>
        /// <param name="ticketId">Identificador único do bilhete.</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckIn(int ticketId)
        {
           
            var isStaff = User.IsInRole("Employee") || User.IsInRole("Admin");

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null)
            {
                TempData["Error"] = "Bilhete não encontrado.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            if (!isStaff)
            {
                // Check-in online: só o próprio passageiro pode fazer check-in do seu bilhete
                var passenger = await GetCurrentPassengerAsync();
                if (passenger == null || ticket.PassengerId != passenger.Id)
                {
                    TempData["Error"] = "Bilhete inválido para check-in.";
                    return RedirectToAction(nameof(Index));
                }
            }
            // Check-in presencial: o funcionário já está autorizado pelo [Authorize] da EmployeeCheckIn,
            // pode processar qualquer bilhete válido apresentado ao balcão.

            if (ticket.Status != "Paid")
            {
                TempData["Error"] = "Este bilhete não está pago ou já tem check-in feito.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            if (ticket.Flight == null)
            {
                TempData["Error"] = "Dados do voo indisponíveis para este bilhete.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            if (ticket.Flight.Status == "Cancelled")
            {
                TempData["Error"] = "Não é possível fazer check-in: o voo foi cancelado.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            // A janela de 48h-1h aplica-se só ao check-in online;
            // ao balcão o funcionário pode processar até à hora de partida.
            // A janela de 48h-1h aplica-se só ao check-in online (regra centralizada no ConverterHelper);
            // ao balcão o funcionário pode processar até à hora de partida.
            if (!isStaff && !_converterHelper.CanCheckInOnline(ticket))
            {
                TempData["Error"] = "Este bilhete não está dentro da janela de check-in online (entre 48h e 1h antes da partida).";
                return RedirectToAction(nameof(Index));
            }

            // Atualiza o estado do bilhete
            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            // Cria o Cartão de Embarque com o gate real do voo
            var gateNumber = ticket.Flight.Gate?.GateNumber ?? "TBA";

            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,
                IssuedAt = DateTime.UtcNow,
                Gate = gateNumber,
                SequenceNumber = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId),
                QRCode = $"BOARDING|{ticket.Id}|{ticket.Flight.FlightNumber}|{gateNumber}"
            };

            await _boardingPassRepository.AddAsync(boardingPass);
            await _boardingPassRepository.SaveAsync();

            TempData["Success"] = "Check-in realizado com sucesso! O cartão de embarque está pronto.";
            return RedirectToAction(nameof(Confirmation), new { boardingPassId = boardingPass.Id });
        }

        /// <summary>
        /// Exibe o resumo e opção de download do cartão de embarque recém-gerado.
        /// </summary>
        /// <param name="boardingPassId">Identificador do cartão de embarque.</param>
        [HttpGet]
        [Authorize]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Confirmation(int boardingPassId)
        {
            var boardingPass = await _boardingPassRepository.GetBoardingPassWithDetailsAsync(boardingPassId);
            if (boardingPass == null) return NotFound();

        
            var qrBytes = _qrCodeService.GenerateQrCode(boardingPass.QRCode ?? "BOARDING");
            ViewBag.QrCodeBase64 = qrBytes.Length > 0 ? Convert.ToBase64String(qrBytes) : null;

            return View(boardingPass);
        }

        /// <summary>
        /// Ponte de conveniência: recebe o Id do bilhete (usado no dashboard do passageiro),
        /// encontra o cartão de embarque correspondente e redireciona para a Confirmation.
        /// </summary>
        /// <param name="ticketId">Identificador do bilhete já com check-in feito.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmationByTicket(int ticketId)
        {
            var boardingPass = await _boardingPassRepository.GetByTicketIdAsync(ticketId);
            if (boardingPass == null) return NotFound();

            return RedirectToAction(nameof(Confirmation), new { boardingPassId = boardingPass.Id });
        }

        /// <summary>
        /// Gera e disponibiliza para transferência o ficheiro PDF do Cartão de Embarque com QR Code.
        /// </summary>
        /// <param name="boardingPassId">Identificador do cartão de embarque.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadBoardingPassPdf(int boardingPassId)
        {
            var boardingPass = await _boardingPassRepository.GetBoardingPassWithDetailsAsync(boardingPassId);
            if (boardingPass == null) return NotFound();

            var isStaff = User.IsInRole("Employee") || User.IsInRole("Admin");
            if (!isStaff)
            {
                // Um passageiro só pode descarregar o seu próprio cartão de embarque
                var passenger = await GetCurrentPassengerAsync();
                if (passenger == null || boardingPass.Ticket?.PassengerId != passenger.Id)
                    return NotFound();
            }

            var pdfBytes = _pdfService.GenerateBoardingPassPdf(boardingPass);
            return File(pdfBytes, "application/pdf", $"CartaoEmbarque_{boardingPass.Id}.pdf");
        }

        /// <summary>
        /// Lista todos os bilhetes/passageiros de um voo específico, para gestão por
        /// Administradores e Funcionários (ex: ver quem falta fazer check-in).
        /// </summary>
        /// <param name="flightId">Identificador do voo.</param>
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> FlightCheckIn(int flightId)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null) return NotFound();

            var tickets = await _ticketRepository.GetByFlightAsync(flightId);

            ViewBag.Flight = flight;
            return View(tickets.OrderBy(t => t.Seat?.Code));
        }
    }
}
