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
    /// Controlador responsável pela gestão operacional de voos e exportação de dados na plataforma LisAeroGest.
    /// Acesso permitido a Administradores e Funcionários.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
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

        public FlightsController(
            IFlightRepository flightRepository,
            IAirlineRepository airlineRepository,
            IAirportRepository airportRepository,
            IAircraftRepository aircraftRepository,
            IGateRepository gateRepository,
            ISeatRepository seatRepository,
            IConverterHelper converterHelper,
            IFlightExportService flightExportService)
        {
            _flightRepository = flightRepository;
            _airlineRepository = airlineRepository;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _gateRepository = gateRepository;
            _seatRepository = seatRepository;
            _converterHelper = converterHelper;
            _flightExportService = flightExportService;
        }

        // ─── INDEX COM FILTROS ───────────────────────────────────────────────

        /// <summary>
        /// Lista os voos com suporte a filtros por companhia, origem, destino e estado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? airlineId, int? originId, int? destinationId, string? status)
        {
            var query = _flightRepository.GetAllQueryable();

            if (airlineId.HasValue) query = query.Where(f => f.AirlineId == airlineId.Value);
            if (originId.HasValue) query = query.Where(f => f.OriginAirportId == originId.Value);
            if (destinationId.HasValue) query = query.Where(f => f.DestinationAirportId == destinationId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(f => f.Status == status);

            var flights = await query.OrderByDescending(f => f.DepartureTime).ToListAsync();

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

        // ─── EXPORTAÇÃO (XML / PDF) ──────────────────────────────────────────

        /// <summary>
        /// Gera e transfere um relatório em formato XML com todos os voos detalhados.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportXml()
        {
            var xmlBytes = await _flightExportService.ExportFlightsToXmlAsync();
            var fileName = $"Voos_LisAeroGest_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xml";
            return File(xmlBytes, "application/xml", fileName);
        }

        /// <summary>
        /// Gera e transfere um relatório em formato PDF com a listagem de voos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _flightExportService.ExportFlightsToPdfAsync();
            var fileName = $"Voos_LisAeroGest_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ─── DETAILS ────────────────────────────────────────────────────────

        /// <summary>
        /// Exibe os detalhes de um voo específico.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        // ─── CREATE ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário para criação de um novo voo (Apenas Admin).
        /// </summary>
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

        /// <summary>
        /// Processa a submissão do formulário de criação de voo.
        /// </summary>
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

            // Gera automaticamente os lugares associados ao voo recém-criado
            await _seatRepository.GenerateSeatsForFlightAsync(flight.Id, viewModel.AircraftId);

            TempData["Success"] = $"Voo {flight.FlightNumber} criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT ───────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de edição de um voo existente (Apenas Admin).
        /// </summary>
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

        /// <summary>
        /// Processa as alterações a um voo existente.
        /// </summary>
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

        // ─── CHANGE STATUS ──────────────────────────────────────────────────

        /// <summary>
        /// Altera o estado operacional do voo a partir da listagem.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var valid = new[] { "Scheduled", "CheckIn", "Boarding", "Departed", "Delayed", "Cancelled" };
            if (!valid.Contains(newStatus))
            {
                TempData["Error"] = "Estado operacional inválido.";
                return RedirectToAction(nameof(Index));
            }

            flight.Status = newStatus;
            await _flightRepository.UpdateAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Estado do Voo {flight.FlightNumber} alterado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── DELETE ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta a página de confirmação para remoção de um voo (Apenas Admin).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        /// <summary>
        /// Executa a remoção do voo, verificando antes se existem assentos/bilhetes vendidos.
        /// </summary>
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
    }
}