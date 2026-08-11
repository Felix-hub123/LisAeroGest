using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Inicializa uma nova instância do controlador <see cref="CheckInController"/>.
        /// </summary>
        /// <param name="ticketRepository">Repositório de bilhetes.</param>
        /// <param name="boardingPassRepository">Repositório para persistência de cartões de embarque.</param>
        /// <param name="passengerRepository">Repositório para dados do passageiro.</param>
        /// <param name="userHelper">Helper de gestão de utilizadores.</param>
        /// <param name="pdfService">Serviço de geração de documentos PDF.</param>
        public CheckInController(
            ITicketRepository ticketRepository,
            IBoardingPassRepository boardingPassRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            PdfService pdfService)
        {
            _ticketRepository = ticketRepository;
            _boardingPassRepository = boardingPassRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _pdfService = pdfService;
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
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null || ticket.PassengerId != passenger.Id || ticket.Status != "Paid")
            {
                TempData["Error"] = "Bilhete inválido para check-in.";
                return RedirectToAction(nameof(Index));
            }

            // Atualiza o estado do bilhete
            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            // Cria o Cartão de Embarque
            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,
                IssuedAt = DateTime.UtcNow,
                Gate = "A12", // Atribuição de portão
                SequenceNumber = Random.Shared.Next(1, 150),
                QRCode = $"BOARDING|{ticket.Id}|{ticket.Flight?.FlightNumber}|A12"
            };

            await _boardingPassRepository.AddAsync(boardingPass);
            await _boardingPassRepository.SaveAsync();

            TempData["Success"] = "Check-in realizado com sucesso! O seu cartão de embarque está pronto.";
            return RedirectToAction(nameof(Confirmation), new { boardingPassId = boardingPass.Id });
        }

        /// <summary>
        /// Exibe o resumo e opção de download do cartão de embarque recém-gerado.
        /// </summary>
        /// <param name="boardingPassId">Identificador do cartão de embarque.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Confirmation(int boardingPassId)
        {
            var boardingPass = await _boardingPassRepository.GetBoardingPassWithDetailsAsync(boardingPassId);
            if (boardingPass == null) return NotFound();

            return View(boardingPass);
        }

        /// <summary>
        /// Gera e disponibiliza para transferência o ficheiro PDF do Cartão de Embarque com QR Code.
        /// </summary>
        /// <param name="boardingPassId">Identificador do cartão de embarque.</param>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadBoardingPassPdf(int boardingPassId)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null) return RedirectToAction("Index", "Home");

            var boardingPass = await _boardingPassRepository.GetBoardingPassWithDetailsAsync(boardingPassId);
            if (boardingPass == null || boardingPass.Ticket?.PassengerId != passenger.Id)
            {
                return NotFound();
            }

            var pdfBytes = _pdfService.GenerateBoardingPassPdf(boardingPass);
            return File(pdfBytes, "application/pdf", $"CartaoEmbarque_{boardingPass.Id}.pdf");
        }
    }
}