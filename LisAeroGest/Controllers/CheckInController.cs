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
    /// Controlador responsável pela gestão do processo de check-in
    /// e emissão dos respetivos cartões de embarque.
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


        // =========================================================
        // HELPER - PASSAGEIRO ATUAL
        // =========================================================

        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (User.Identity?.Name == null)
            {
                return null;
            }

            var user =
                await _userHelper
                    .GetUserByEmailAsync(
                        User.Identity.Name);

            if (user == null)
            {
                return null;
            }

            return await _passengerRepository
                .GetByUserIdAsync(
                    user.Id);
        }


        // =========================================================
        // CHECK-IN DO PASSAGEIRO
        // =========================================================

        /// <summary>
        /// Lista os bilhetes pagos do passageiro autenticado.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var passenger =
                await GetCurrentPassengerAsync();

            if (passenger == null)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }


            var tickets =
                (await _ticketRepository
                    .GetByPassengerAsync(
                        passenger.Id))
                .Where(t =>
                    t.Status == "Paid")
                .ToList();


            return View(tickets);
        }


        // =========================================================
        // BALCÃO CHECK-IN - FUNCIONÁRIO
        // =========================================================

        /// <summary>
        /// Abre o balcão de check-in presencial.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Employee,Admin")]
        public IActionResult EmployeeCheckIn()
        {
            return View(
                new List<Ticket>());
        }


        /// <summary>
        /// Pesquisa passageiros/bilhetes para check-in presencial.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Employee,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeCheckIn(
            string searchTerm)
        {
            // =====================================================
            // PESQUISA VAZIA
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                searchTerm))
            {
                TempData["Error"] =
                    "Por favor, introduza um termo de pesquisa.";

                return View(
                    new List<Ticket>());
            }


            searchTerm =
                searchTerm.Trim();


            // =====================================================
            // PESQUISAR BILHETES
            // =====================================================

            var results =
                await _ticketRepository
                    .SearchForCheckInAsync(
                        searchTerm);


            // =====================================================
            // SEM RESULTADOS
            // =====================================================

            if (!results.Any())
            {
                ViewBag.SearchTerm =
                    searchTerm;

                ViewBag.ResultCount =
                    0;


                TempData["Error"] =
                    $"Nenhum bilhete encontrado para \"{searchTerm}\".";


                return View(
                    new List<Ticket>());
            }


            // =====================================================
            // CARREGAR DETALHES
            // =====================================================

            var tickets =
                new List<Ticket>();


            foreach (var result in results)
            {
                var fullTicket =
                    await _ticketRepository
                        .GetTicketWithDetailsAsync(
                            result.Id);


                if (fullTicket != null)
                {
                    tickets.Add(
                        fullTicket);
                }
            }


            // =====================================================
            // DADOS PARA A VIEW
            // =====================================================

            ViewBag.SearchTerm =
                searchTerm;

            ViewBag.ResultCount =
                tickets.Count;


            return View(tickets);
        }


        // =========================================================
        // PROCESSAR CHECK-IN
        // =========================================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckIn(
            int ticketId)
        {
            var isStaff =
                User.IsInRole("Employee") ||
                User.IsInRole("Admin");


            // =====================================================
            // CARREGAR BILHETE
            // =====================================================

            var ticket =
                await _ticketRepository
                    .GetTicketWithDetailsAsync(
                        ticketId);


            if (ticket == null)
            {
                TempData["Error"] =
                    "Bilhete não encontrado.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            // =====================================================
            // IMPEDIR CHECK-IN DUPLICADO
            // =====================================================

            var existingBoardingPass =
                await _boardingPassRepository
                    .GetByTicketIdAsync(
                        ticketId);


            if (existingBoardingPass != null)
            {
                TempData["Info"] =
                    "Este bilhete já tem check-in feito. A mostrar o cartão de embarque existente.";


                return RedirectToAction(
                    nameof(Confirmation),
                    new
                    {
                        boardingPassId =
                            existingBoardingPass.Id
                    });
            }


            // =====================================================
            // VALIDAR PROPRIETÁRIO DO BILHETE
            // =====================================================

            if (!isStaff)
            {
                var passenger =
                    await GetCurrentPassengerAsync();


                if (passenger == null ||
                    ticket.PassengerId !=
                    passenger.Id)
                {
                    TempData["Error"] =
                        "Bilhete inválido para check-in.";


                    return RedirectToAction(
                        nameof(Index));
                }
            }


            // =====================================================
            // VALIDAR ESTADO DO BILHETE
            // =====================================================

            if (ticket.Status != "Paid")
            {
                TempData["Error"] =
                    "Este bilhete não está pago ou já tem check-in feito.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            // =====================================================
            // VALIDAR VOO
            // =====================================================

            if (ticket.Flight == null)
            {
                TempData["Error"] =
                    "Dados do voo indisponíveis para este bilhete.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            if (ticket.Flight.Status ==
                "Cancelled")
            {
                TempData["Error"] =
                    "Não é possível fazer check-in: o voo foi cancelado.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            if (ticket.Flight.Status ==
                "Departed")
            {
                TempData["Error"] =
                    "Não é possível fazer check-in: o voo já partiu.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            if (ticket.Flight.DepartureTime <
                DateTime.UtcNow)
            {
                TempData["Error"] =
                    "Não é possível fazer check-in: a hora de partida já passou.";


                return RedirectToAction(
                    isStaff
                        ? nameof(EmployeeCheckIn)
                        : nameof(Index));
            }


            // =====================================================
            // JANELA DE CHECK-IN ONLINE
            // APENAS PASSAGEIRO
            // =====================================================

            if (!isStaff &&
                !_converterHelper
                    .CanCheckInOnline(
                        ticket))
            {
                TempData["Error"] =
                    "Este bilhete não está dentro da janela de check-in online (entre 48h e 1h antes da partida).";


                return RedirectToAction(
                    nameof(Index));
            }


            // =====================================================
            // CRIAR BOARDING PASS
            // =====================================================

            var gateNumber =
                ticket.Flight.Gate?
                    .GateNumber
                ?? "TBA";


            var sequenceNumber =
                await _boardingPassRepository
                    .GetNextSequenceNumberAsync(
                        ticket.FlightId);


            var boardingPass =
                new BoardingPass
                {
                    TicketId =
                        ticket.Id,

                    IssuedAt =
                        DateTime.UtcNow,

                    Gate =
                        gateNumber,

                    SequenceNumber =
                        sequenceNumber,

                    QRCode =
                        $"BOARDING|{ticket.Id}|{ticket.Flight.FlightNumber}|{gateNumber}",

                    IssuedByUserId =
                        User.FindFirst(
                            System.Security.Claims
                                .ClaimTypes
                                .NameIdentifier)?
                            .Value,

                    IssuedByName =
                        User.Identity?.Name
                };


            await _boardingPassRepository
                .AddAsync(
                    boardingPass);


            // =====================================================
            // ATUALIZAR ESTADO DO BILHETE
            // =====================================================

            ticket.Status =
                "CheckedIn";


            await _ticketRepository
                .UpdateAsync(
                    ticket);


            await _boardingPassRepository
                .SaveAsync();


            // =====================================================
            // NOTIFICAÇÃO + EMAIL
            // =====================================================

            try
            {
                var passenger =
                    ticket.Passenger;


                // -------------------------------------------------
                // NOTIFICAÇÃO IN-APP
                // -------------------------------------------------

                if (passenger?.UserId != null)
                {
                    await _notificationRepository
                        .AddAsync(
                            new Notification
                            {
                                UserId =
                                    passenger.UserId,

                                Title =
                                    $"Check-in confirmado — Voo {ticket.Flight.FlightNumber}",

                                Message =
                                    $"O seu check-in foi efetuado. Lugar {ticket.Seat?.Code}, Gate {gateNumber}, Sequência {boardingPass.SequenceNumber}.",

                                Icon =
                                    "bi-check-circle-fill",

                                ColorClass =
                                    "text-success",

                                CreatedAt =
                                    DateTime.UtcNow,

                                IsRead =
                                    false
                            });


                    await _notificationRepository
                        .SaveAsync();
                }


                // -------------------------------------------------
                // EMAIL
                // -------------------------------------------------

                if (!string.IsNullOrEmpty(
                    passenger?.Email))
                {
                    var bpUrl =
                        Url.Action(
                            "Confirmation",
                            "CheckIn",
                            new
                            {
                                boardingPassId =
                                    boardingPass.Id
                            },
                            Request.Scheme);


                    var emailBody =
                        $@"
<div style='font-family: Arial, sans-serif;
            max-width: 600px;
            margin: 0 auto;
            color: #333;'>

    <h2 style='color: #1F5C99;
               border-bottom: 3px solid #c8e629;
               padding-bottom: 8px;'>

        ✅ Check-in Confirmado

    </h2>

    <p>
        Olá
        <strong>
            {passenger.FirstName}
        </strong>,
    </p>

    <p>
        O seu check-in foi efetuado com sucesso.
    </p>

    <div style='background: #f5f5f5;
                padding: 20px;
                border-radius: 8px;
                margin: 20px 0;
                border-left: 4px solid #1F5C99;'>

        <p>
            <strong>Voo:</strong>
            {ticket.Flight.FlightNumber}
        </p>

        <p>
            <strong>Rota:</strong>
            {ticket.Flight.OriginAirport?.IATACode}
            →
            {ticket.Flight.DestinationAirport?.IATACode}
        </p>

        <p>
            <strong>Partida:</strong>
            {ticket.Flight.DepartureTime:dd/MM/yyyy HH:mm}
        </p>

        <p>
            <strong>Lugar:</strong>
            {ticket.Seat?.Code}
        </p>

        <p>
            <strong>Gate:</strong>
            {gateNumber}
        </p>

        <p>
            <strong>Sequência:</strong>
            {boardingPass.SequenceNumber}
        </p>

    </div>

    <p style='text-align: center;
              margin: 30px 0;'>

        <a href='{bpUrl}'
           style='background-color: #c8e629;
                  color: #0a0e17;
                  padding: 14px 28px;
                  text-decoration: none;
                  border-radius: 8px;
                  font-weight: bold;
                  display: inline-block;'>

            📥 Ver cartão de embarque

        </a>

    </p>

    <p>
        Cumprimentos,
        <br />

        <strong>
            LisAeroGest
        </strong>

        — Aeroporto de Lisboa

    </p>

</div>";


                    await _mailHelper
                        .SendEmailAsync(
                            passenger.Email,
                            $"Check-in confirmado — Voo {ticket.Flight.FlightNumber}",
                            emailBody);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug
                    .WriteLine(
                        $"[CheckIn] Erro ao notificar: {ex.Message}");

                // O check-in não deve falhar
                // por causa da notificação/email.
            }


            // =====================================================
            // CONCLUSÃO
            // =====================================================

            TempData["Success"] =
                "Check-in realizado com sucesso!";


            return RedirectToAction(
                nameof(Confirmation),
                new
                {
                    boardingPassId =
                        boardingPass.Id
                });
        }


        // =========================================================
        // CONFIRMAÇÃO
        // =========================================================

        /// <summary>
        /// Mostra o cartão de embarque após o check-in.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Confirmation(
            int boardingPassId)
        {
            var boardingPass =
                await _boardingPassRepository
                    .GetBoardingPassWithDetailsAsync(
                        boardingPassId);


            if (boardingPass == null)
            {
                return NotFound();
            }


            var qrBytes =
                _qrCodeService
                    .GenerateQrCode(
                        boardingPass.QRCode ??
                        "BOARDING");


            ViewBag.QrCodeBase64 =
                qrBytes.Length > 0
                    ? Convert.ToBase64String(
                        qrBytes)
                    : null;


            return View(
                boardingPass);
        }


        // =========================================================
        // CONFIRMAÇÃO POR BILHETE
        // =========================================================

        /// <summary>
        /// Procura o cartão de embarque pelo Id do bilhete.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmationByTicket(
            int ticketId)
        {
            var boardingPass =
                await _boardingPassRepository
                    .GetByTicketIdAsync(
                        ticketId);


            if (boardingPass == null)
            {
                return NotFound();
            }


            return RedirectToAction(
                nameof(Confirmation),
                new
                {
                    boardingPassId =
                        boardingPass.Id
                });
        }


        // =========================================================
        // DOWNLOAD PDF
        // =========================================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadBoardingPassPdf(
            int boardingPassId)
        {
            var boardingPass =
                await _boardingPassRepository
                    .GetBoardingPassWithDetailsAsync(
                        boardingPassId);


            if (boardingPass == null)
            {
                return NotFound();
            }


            var isStaff =
                User.IsInRole("Employee") ||
                User.IsInRole("Admin");


            // =====================================================
            // PASSAGEIRO SÓ PODE VER O SEU CARTÃO
            // =====================================================

            if (!isStaff)
            {
                var passenger =
                    await GetCurrentPassengerAsync();


                if (passenger == null ||
                    boardingPass.Ticket?
                        .PassengerId !=
                    passenger.Id)
                {
                    return NotFound();
                }
            }


            var pdfBytes =
                _pdfService
                    .GenerateBoardingPassPdf(
                        boardingPass);


            return File(
                pdfBytes,
                "application/pdf",
                $"CartaoEmbarque_{boardingPass.Id}.pdf");
        }


        // =========================================================
        // CHECK-IN POR VOO
        // =========================================================

        /// <summary>
        /// Mostra os passageiros/bilhetes de um voo específico.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> FlightCheckIn(
            int flightId)
        {
            var flight =
                await _flightRepository
                    .GetWithDetailsAsync(
                        flightId);


            if (flight == null)
            {
                return NotFound();
            }


            var tickets =
                await _ticketRepository
                    .GetByFlightAsync(
                        flightId);


            ViewBag.Flight =
                flight;


            return View(
                tickets.OrderBy(
                    t => t.Seat?.Code));
        }
    }
}