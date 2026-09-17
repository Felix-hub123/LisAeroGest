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
        private readonly INotificationRepository _notificationRepository;
        private readonly IMailHelper _mailHelper;

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
            IConverterHelper converterHelper,
            INotificationRepository notificationRepository,
            IMailHelper mailHelper)
        {
            _ticketRepository = ticketRepository;
            _boardingPassRepository = boardingPassRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _pdfService = pdfService;
            _qrCodeService = qrCodeService;
            _flightRepository = flightRepository;
            _converterHelper = converterHelper;
            _notificationRepository = notificationRepository;
            _mailHelper = mailHelper;
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
                TempData["Error"] = "Por favor, introduza um termo de pesquisa.";
                return View();
            }

            // 🔥 Pesquisa otimizada (query única na DB)
            var results = await _ticketRepository.SearchForCheckInAsync(searchTerm);

            if (!results.Any())
            {
                TempData["Error"] = $"Nenhum bilhete encontrado para \"{searchTerm}\".";
                return View();
            }

            // 🅰️ 1 resultado → mostra logo
            if (results.Count == 1)
            {
                var single = results.First();
                var fullTicket = await _ticketRepository.GetTicketWithDetailsAsync(single.Id);

                if (fullTicket!.HasCheckedIn)
                {
                    TempData["Warning"] = "Este passageiro já efetuou o check-in.";
                }

                return View(fullTicket);
            }

            // 🅱️ Vários resultados → mostra lista
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ResultCount = results.Count;

            return View("EmployeeCheckInResults", results);
        }

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

            // 🔥 Impedir double check-in
            var existingBoardingPass = await _boardingPassRepository.GetByTicketIdAsync(ticketId);
            if (existingBoardingPass != null)
            {
                TempData["Info"] = "Este bilhete já tem check-in feito. A mostrar o cartão de embarque existente.";
                return RedirectToAction(nameof(Confirmation), new { boardingPassId = existingBoardingPass.Id });
            }

            // Validações base
            if (!isStaff)
            {
                var passenger = await GetCurrentPassengerAsync();
                if (passenger == null || ticket.PassengerId != passenger.Id)
                {
                    TempData["Error"] = "Bilhete inválido para check-in.";
                    return RedirectToAction(nameof(Index));
                }
            }

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

            if (ticket.Flight.Status == "Departed")
            {
                TempData["Error"] = "Não é possível fazer check-in: o voo já partiu.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            if (ticket.Flight.DepartureTime < DateTime.UtcNow)
            {
                TempData["Error"] = "Não é possível fazer check-in: a hora de partida já passou.";
                return RedirectToAction(isStaff ? nameof(EmployeeCheckIn) : nameof(Index));
            }

            if (!isStaff && !_converterHelper.CanCheckInOnline(ticket))
            {
                TempData["Error"] = "Este bilhete não está dentro da janela de check-in online (entre 48h e 1h antes da partida).";
                return RedirectToAction(nameof(Index));
            }

            // ═══════════════════════════════════════════════════════════
            // Criar BoardingPass
            // ═══════════════════════════════════════════════════════════

            var gateNumber = ticket.Flight.Gate?.GateNumber ?? "TBA";

            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,
                IssuedAt = DateTime.UtcNow,
                Gate = gateNumber,
                SequenceNumber = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId),
                QRCode = $"BOARDING|{ticket.Id}|{ticket.Flight.FlightNumber}|{gateNumber}",

                // 🔥 Auditoria
                IssuedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                IssuedByName = User.Identity?.Name
            };

            await _boardingPassRepository.AddAsync(boardingPass);

            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            await _boardingPassRepository.SaveAsync();

            // ═══════════════════════════════════════════════════════════
            // 🔥 Notificações (in-app + email)
            // ═══════════════════════════════════════════════════════════

            try
            {
                var passenger = ticket.Passenger;

                // Notificação in-app (só se o passageiro tiver conta)
                if (passenger?.UserId != null)
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        UserId = passenger.UserId,
                        Title = $"Check-in confirmado — Voo {ticket.Flight.FlightNumber}",
                        Message = $"O seu check-in foi efetuado. Lugar {ticket.Seat?.Code}, Gate {gateNumber}, Sequência {boardingPass.SequenceNumber}.",
                        Icon = "bi-check-circle-fill",
                        ColorClass = "text-success",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                    await _notificationRepository.SaveAsync();
                }

                // Email (se tiver email)
                if (!string.IsNullOrEmpty(passenger?.Email))
                {
                    var bpUrl = Url.Action("Confirmation", "CheckIn",
                        new { boardingPassId = boardingPass.Id }, Request.Scheme);

                    var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #1F5C99; border-bottom: 3px solid #c8e629; padding-bottom: 8px;'>
                        ✅ Check-in Confirmado
                    </h2>
                    <p>Olá <strong>{passenger.FirstName}</strong>,</p>
                    <p>O seu check-in foi efetuado com sucesso.</p>
                    
                    <div style='background: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #1F5C99;'>
                        <p><strong>Voo:</strong> {ticket.Flight.FlightNumber}</p>
                        <p><strong>Rota:</strong> {ticket.Flight.OriginAirport?.IATACode} → {ticket.Flight.DestinationAirport?.IATACode}</p>
                        <p><strong>Partida:</strong> {ticket.Flight.DepartureTime:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Lugar:</strong> {ticket.Seat?.Code}</p>
                        <p><strong>Gate:</strong> {gateNumber}</p>
                        <p><strong>Sequência:</strong> {boardingPass.SequenceNumber}</p>
                    </div>
                    
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{bpUrl}' 
                           style='background-color: #c8e629; color: #0a0e17; padding: 14px 28px; 
                                  text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                            📥 Ver cartão de embarque
                        </a>
                    </p>
                    
                    <p>Cumprimentos,<br/><strong>LisAeroGest</strong> — Aeroporto de Lisboa</p>
                </div>";

                    await _mailHelper.SendEmailAsync(
                        passenger.Email,
                        $"Check-in confirmado — Voo {ticket.Flight.FlightNumber}",
                        emailBody);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckIn] Erro ao notificar: {ex.Message}");
                // Não falha o check-in por causa de notificações
            }

            TempData["Success"] = "Check-in realizado com sucesso!";
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
