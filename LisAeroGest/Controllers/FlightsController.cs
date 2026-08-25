using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão operacional de voos e consulta pública no LisAeroGest.
    /// </summary>
    public class FlightsController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirlineRepository _airlineRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IGateRepository _gateRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IFlightExportService _flightExportService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMailHelper _emailHelper;

        public FlightsController(
            IFlightRepository flightRepository,
            IAirlineRepository airlineRepository,
            IAirportRepository airportRepository,
            IAircraftRepository aircraftRepository,
            IGateRepository gateRepository,
            ISeatRepository seatRepository,
            IConverterHelper converterHelper,
            IFlightExportService flightExportService,
            INotificationRepository notificationRepository,
            ITicketRepository ticketRepository,
            IMailHelper emailHelper)
        {
            _flightRepository = flightRepository;
            _airlineRepository = airlineRepository;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _gateRepository = gateRepository;
            _seatRepository = seatRepository;
            _converterHelper = converterHelper;
            _flightExportService = flightExportService;
            _notificationRepository = notificationRepository;
            _ticketRepository = ticketRepository;
            _emailHelper = emailHelper;
        }

        // ─── INDEX PÚBLICO COM FILTROS ───────────────────────────────────────

        /// <summary>
        /// Lista os voos com suporte a filtros. Acesso livre a visitantes.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(int? airlineId, int? originId, int? destinationId, string? status)
        {
            var query = _flightRepository.GetAllQueryable();

            if (airlineId.HasValue) query = query.Where(f => f.AirlineId == airlineId.Value);
            if (originId.HasValue) query = query.Where(f => f.OriginAirportId == originId.Value);
            if (destinationId.HasValue) query = query.Where(f => f.DestinationAirportId == destinationId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(f => f.Status == status);

            var flights = await query
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .OrderByDescending(f => f.DepartureTime)
                .ToListAsync();

            var model = new FlightFilterViewModel
            {
                Flights = flights,
                Airlines = _converterHelper.ToComboAirlines(await _airlineRepository.GetAllAsync(), airlineId),
                Airports = _converterHelper.ToComboAirports(await _airportRepository.GetAllAsync(), originId),
                Statuses = _converterHelper.ToComboStatuses(status),
                FilterAirlineId = airlineId,
                FilterOriginId = originId,
                FilterDestinationId = destinationId,
                FilterStatus = status
            };

            ViewBag.Destinations = _converterHelper.ToComboAirports(await _airportRepository.GetAllAsync(), destinationId);

            return View(model);
        }

        // ─── AÇÃO DE COMPRA (NOVO) ──────────────────────────────────────────

        /// <summary>
        /// Ação de compra de bilhete - EXIGE AUTENTICAÇÃO.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Purchase(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            // Verifica se o voo está disponível para compra
            if (flight.Status == "Cancelled" || flight.Status == "Departed")
            {
                TempData["Error"] = "Este voo não está disponível para compra.";
                return RedirectToAction(nameof(Index));
            }

            if (flight.DepartureTime < DateTime.Now.AddHours(1))
            {
                TempData["Error"] = "A compra de bilhetes para este voo já não está disponível (menos de 1 hora para a partida).";
                return RedirectToAction(nameof(Index));
            }

            // Verifica se o utilizador já tem bilhete para este voo
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var existingTicket = await _ticketRepository.GetAllQueryable()
                .AnyAsync(t => t.FlightId == id && t.Passenger.UserId == userId && t.Status != "Cancelled");

            if (existingTicket)
            {
                TempData["Error"] = "Já possui um bilhete para este voo.";
                return RedirectToAction(nameof(Index));
            }

            // Redireciona para o fluxo de checkout (Booking/SelectSeat)
            // NOTA: Assumindo que tens um controller Booking com ação SelectSeat
            return RedirectToAction("SelectSeat", "Booking", new { flightId = id });
        }

        // ─── EXPORTAÇÃO (PROTEGIDO) ──────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ExportXml()
        {
            var xmlBytes = await _flightExportService.ExportFlightsToXmlAsync();
            var fileName = $"Voos_LisAeroGest_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xml";
            return File(xmlBytes, "application/xml", fileName);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _flightExportService.ExportFlightsToPdfAsync();
            var fileName = $"Voos_LisAeroGest_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ─── DETAILS PÚBLICO ────────────────────────────────────────────────

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        // ─── CREATE (PROTEGIDO) ─────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new FlightViewModel
            {
                DepartureTime = DateTime.Now.AddHours(2),
                ArrivalTime = DateTime.Now.AddHours(4),
            };

            await PopulateDropdownsAsync(vm, "Scheduled");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(FlightViewModel viewModel)
        {
            if (viewModel.DepartureTime <= DateTime.Now)
                ModelState.AddModelError("DepartureTime", "A hora de partida não pode ser no passado.");

            if (viewModel.ArrivalTime <= viewModel.DepartureTime)
                ModelState.AddModelError("ArrivalTime", "A hora de chegada tem de ser posterior à partida.");

            if (viewModel.OriginAirportId == viewModel.DestinationAirportId)
                ModelState.AddModelError("DestinationAirportId", "O aeroporto de destino tem de ser diferente da origem.");

            if (viewModel.GateId.HasValue && viewModel.GateId.Value > 0)
            {
                if (await _gateRepository.IsGateOccupiedAsync(viewModel.GateId.Value, viewModel.DepartureTime, viewModel.ArrivalTime))
                    ModelState.AddModelError("GateId", "O Gate selecionado já está ocupado por outro voo neste horário.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel, viewModel.Status);
                return View(viewModel);
            }

            var flight = _converterHelper.ToFlight(viewModel, isEdit: false);

            await _flightRepository.AddAsync(flight);
            await _flightRepository.SaveAsync();

            await _seatRepository.GenerateSeatsForFlightAsync(flight.Id, viewModel.AircraftId);

            TempData["Success"] = $"Voo {flight.FlightNumber} criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT (PROTEGIDO) ───────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            var vm = _converterHelper.ToFlightViewModel(flight);
            await PopulateDropdownsAsync(vm, flight.Status);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(FlightViewModel viewModel)
        {
            if (viewModel.ArrivalTime <= viewModel.DepartureTime)
                ModelState.AddModelError("ArrivalTime", "A chegada tem de ser depois da partida.");

            if (viewModel.OriginAirportId == viewModel.DestinationAirportId)
                ModelState.AddModelError("DestinationAirportId", "O destino tem de ser diferente da origem.");

            if (viewModel.GateId.HasValue && viewModel.GateId.Value > 0)
            {
                if (await _gateRepository.IsGateOccupiedAsync(viewModel.GateId.Value, viewModel.DepartureTime, viewModel.ArrivalTime, viewModel.Id))
                    ModelState.AddModelError("GateId", "O Gate selecionado já está ocupado por outro voo neste horário.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel, viewModel.Status);
                return View(viewModel);
            }

            var flight = await _flightRepository.GetByIdAsync(viewModel.Id);
            if (flight == null) return NotFound();

            var updatedFlight = _converterHelper.ToFlight(viewModel, isEdit: true);

            flight.FlightNumber = updatedFlight.FlightNumber;
            flight.AirlineId = updatedFlight.AirlineId;
            flight.OriginAirportId = updatedFlight.OriginAirportId;
            flight.DestinationAirportId = updatedFlight.DestinationAirportId;
            flight.AircraftId = updatedFlight.AircraftId;
            flight.GateId = updatedFlight.GateId;
            flight.DepartureTime = updatedFlight.DepartureTime;
            flight.ArrivalTime = updatedFlight.ArrivalTime;
            flight.BasePrice = updatedFlight.BasePrice;
            flight.Status = updatedFlight.Status;

            await _flightRepository.UpdateAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── CHANGE STATUS (PROTEGIDO) ───────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var validStatuses = new[] { "Scheduled", "CheckIn", "Boarding", "Departed", "Delayed", "Cancelled" };
            if (!validStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Estado operacional inválido.";
                return RedirectToAction(nameof(Index));
            }

            var previousStatus = flight.Status;
            flight.Status = newStatus;

            await _flightRepository.UpdateAsync(flight);
            await _flightRepository.SaveAsync();

            if ((newStatus == "Delayed" || newStatus == "Cancelled") && previousStatus != newStatus)
            {
                await NotifyPassengersAboutStatusChangeAsync(flight, newStatus);
            }

            TempData["Success"] = $"Estado do Voo {flight.FlightNumber} alterado para {newStatus} com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── DELETE (PROTEGIDO) ─────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            var hasSoldSeats = await _seatRepository.GetAllQueryable()
                .AnyAsync(s => s.FlightId == id && !s.IsAvailable);

            if (hasSoldSeats || (flight.Seats != null && flight.Seats.Any(s => !s.IsAvailable)))
            {
                TempData["Error"] = "Não é possível eliminar este voo pois já existem bilhetes vendidos.";
                return RedirectToAction(nameof(Index));
            }

            await _flightRepository.DeleteAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} removido com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // ─── MÉTODOS AUXILIARES ─────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(FlightViewModel vm, string? selectedStatus)
        {
            vm.Airlines = _converterHelper.ToComboAirlines(await _airlineRepository.GetAllAsync(), vm.AirlineId);
            vm.Airports = _converterHelper.ToComboAirports(await _airportRepository.GetAllAsync(), vm.OriginAirportId);
            vm.Aircrafts = _converterHelper.ToComboAircrafts(await _aircraftRepository.GetAllAsync(), vm.AircraftId);
            vm.Gates = _converterHelper.ToComboGates(await _gateRepository.GetAllAsync(), vm.GateId);
            vm.Statuses = _converterHelper.ToComboStatuses(selectedStatus);
        }

        private async Task NotifyPassengersAboutStatusChangeAsync(Flight flight, string newStatus)
        {
            var tickets = await _ticketRepository.GetByFlightIdAsync(flight.Id);
            var validTickets = tickets.Where(t => t.Status == "Paid" || t.Status == "CheckedIn").ToList();

            foreach (var ticket in validTickets)
            {
                if (ticket.Passenger?.User?.Email == null) continue;

                var passengerName = $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}";
                var passengerEmail = ticket.Passenger.User.Email;

                string subject;
                string body;

                if (newStatus == "Delayed")
                {
                    subject = $"[LisAeroGest] AVISO: Atraso no voo {flight.FlightNumber}";
                    body = $"<p>Olá <b>{passengerName}</b>,</p>" +
                           $"<p>Informativo: O seu voo <b>{flight.FlightNumber}</b> com destino a " +
                           $"<b>{flight.DestinationAirport?.City}</b> encontra-se <span style='color:red;'>ATRASADO</span>.</p>" +
                           $"<p>Por favor, acompanhe o painel de saídas para atualizações dos novos horários.</p>";
                }
                else
                {
                    subject = $"[LisAeroGest] IMPORTANTE: Cancelamento do voo {flight.FlightNumber}";
                    body = $"<p>Olá <b>{passengerName}</b>,</p>" +
                           $"<p>Lamentamos informar que o seu voo <b>{flight.FlightNumber}</b> foi <span style='color:red;'>CANCELADO</span>.</p>" +
                           $"<p>Por favor, dirija-se ao balcão de apoio ao cliente ou aceda à sua área reservada para reagendamento.</p>";
                }

                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = ticket.Passenger.UserId,
                    Title = subject,
                    Message = body,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });

                await _emailHelper.SendEmailAsync(passengerEmail, subject, body);
            }

            await _notificationRepository.SaveAsync();
        }
    }
}