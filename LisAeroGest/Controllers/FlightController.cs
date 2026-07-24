using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão operacional de voos.
    /// Acesso restrito a Administradores e Funcionários.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class FlightController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirlineRepository _airlineRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IGateRepository _gateRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa o FlightController com os repositórios e helpers necessários.
        /// </summary>
        public FlightController(
            IFlightRepository flightRepository,
            IAirlineRepository airlineRepository,
            IAirportRepository airportRepository,
            IAircraftRepository aircraftRepository,
            IGateRepository gateRepository,
            ISeatRepository seatRepository,
            IConverterHelper converterHelper)
        {
            _flightRepository = flightRepository;
            _airlineRepository = airlineRepository;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _gateRepository = gateRepository;
            _seatRepository = seatRepository;
            _converterHelper = converterHelper;
        }

        // ─── INDEX COM FILTROS ───────────────────────────────────────────────

        /// <summary>
        /// Lista todos os voos, com filtros opcionais por companhia, origem, destino e estado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            int? airlineId, int? originId, int? destinationId, string? status)
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
                Airlines = await GetAirlinesAsync(airlineId),
                Airports = await GetAirportsAsync(originId),
                Statuses = GetStatuses(status),
                FilterAirlineId = airlineId,
                FilterOriginId = originId,
                FilterDestinationId = destinationId,
                FilterStatus = status
            };

            ViewBag.Destinations = await GetAirportsAsync(destinationId);

            return View(model);
        }

        // ─── CREATE ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de criação de voo com as listas de seleção carregadas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new FlightViewModel
            {
                Airlines = await GetAirlinesAsync(),
                Airports = await GetAirportsAsync(),
                Aircrafts = await GetAircraftsAsync(),
                Gates = await GetGatesAsync(),
                Statuses = GetStatuses("Scheduled")
            };

            return View(model);
        }

        /// <summary>
        /// Processa a criação de um novo voo, validando regras de negócio e gerando os lugares.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightViewModel model)
        {
            // Validações de Negócio (Guard Clauses)
            if (model.DepartureTime <= DateTime.Now)
            {
                ModelState.AddModelError("DepartureTime", "A hora de partida deve ser no futuro.");
            }

            if (model.OriginAirportId == model.DestinationAirportId)
            {
                ModelState.AddModelError("DestinationAirportId", "O destino não pode ser igual à origem.");
            }

            if (model.ArrivalTime <= model.DepartureTime)
            {
                ModelState.AddModelError("ArrivalTime", "A chegada tem de ser depois da partida.");
            }

            if (model.GateId.HasValue && model.GateId.Value > 0)
            {
                var isGateOccupied = await _gateRepository.IsGateOccupiedAsync(model.GateId.Value, model.DepartureTime, model.ArrivalTime);
                if (isGateOccupied)
                {
                    ModelState.AddModelError("GateId", "Este portão já se encontra ocupado por outro voo no horário selecionado.");
                }
            }

            if (!ModelState.IsValid)
            {
                await RepopulateDropdownsAsync(model, model.Status);
                return View(model);
            }

            var flight = _converterHelper.ToFlight(model, isEdit: false);

            await _flightRepository.AddAsync(flight);
            await _flightRepository.SaveAsync();

            // Gera os lugares do voo a partir do template da aeronave
            await _seatRepository.GenerateSeatsForFlightAsync(flight.Id, model.AircraftId);

            TempData["Success"] = "Voo criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT ───────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de edição de um voo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var model = _converterHelper.ToFlightViewModel(flight);
            await RepopulateDropdownsAsync(model, flight.Status);

            return View(model);
        }

        /// <summary>
        /// Processa a atualização dos dados de um voo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightViewModel model)
        {
            if (model.OriginAirportId == model.DestinationAirportId)
            {
                ModelState.AddModelError("DestinationAirportId", "O destino não pode ser igual à origem.");
            }

            if (model.ArrivalTime <= model.DepartureTime)
            {
                ModelState.AddModelError("ArrivalTime", "A chegada tem de ser depois da partida.");
            }

            if (!ModelState.IsValid)
            {
                await RepopulateDropdownsAsync(model, model.Status);
                return View(model);
            }

            var flight = await _flightRepository.GetByIdAsync(model.Id);
            if (flight == null) return NotFound();

            // Atualiza a entidade a partir do ViewModel
            var updatedFlight = _converterHelper.ToFlight(model, isEdit: true);

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

            TempData["Success"] = "Voo atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── DETAILS ────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta os detalhes completos de um voo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        // ─── CHANGE STATUS (rápido, via dropdown na lista) ──────────────────

        /// <summary>
        /// Altera rapidamente o estado operacional de um voo a partir da listagem.
        /// </summary>
        /// <param name="id">Identificador do voo.</param>
        /// <param name="newStatus">Novo estado a aplicar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var validStatuses = new[] { "Scheduled", "CheckIn", "Boarding", "Departed", "Delayed", "Cancelled" };
            if (!validStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Estado inválido.";
                return RedirectToAction(nameof(Index));
            }

            flight.Status = newStatus;
            await _flightRepository.UpdateAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} → {FlightStatusHelper.GetStatusText(newStatus)}";
            return RedirectToAction(nameof(Index));
        }

        // ─── DELETE (com confirmação) ───────────────────────────────────────

        /// <summary>
        /// Apresenta a página de confirmação de eliminação de um voo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            return View(flight);
        }

        /// <summary>
        /// Processa a eliminação (soft delete) do voo, impedindo se existirem lugares vendidos.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            var hasSoldSeats = await _seatRepository.GetAllQueryable()
                .AnyAsync(s => s.FlightId == id && !s.IsAvailable);

            if (hasSoldSeats)
            {
                TempData["Error"] = "Não é possível eliminar um voo com bilhetes vendidos.";
                return RedirectToAction(nameof(Index));
            }

            await _flightRepository.DeleteAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = "Voo eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ─── MÉTODOS AUXILIARES ─────────────────────────────────────────────

        /// <summary>
        /// Repopula todos os dropdowns do ViewModel.
        /// </summary>
        private async Task RepopulateDropdownsAsync(FlightViewModel model, string? selectedStatus)
        {
            model.Airlines = await GetAirlinesAsync(model.AirlineId);
            model.Airports = await GetAirportsAsync(model.OriginAirportId);
            model.Aircrafts = await GetAircraftsAsync(model.AircraftId);
            model.Gates = await GetGatesAsync(model.GateId);
            model.Statuses = GetStatuses(selectedStatus);
        }

        private async Task<IEnumerable<SelectListItem>> GetAirlinesAsync(int? selectedId = null)
        {
            var list = await _airlineRepository.GetAllAsync();
            return list.Select(a => new SelectListItem
            {
                Text = $"{a.IATACode} — {a.Name}",
                Value = a.Id.ToString(),
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            }).OrderBy(a => a.Text);
        }

        private async Task<IEnumerable<SelectListItem>> GetAirportsAsync(int? selectedId = null)
        {
            var list = await _airportRepository.GetAllAsync();
            return list.Select(a => new SelectListItem
            {
                Text = $"{a.City} ({a.IATACode})",
                Value = a.Id.ToString(),
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            }).OrderBy(a => a.Text);
        }

        private async Task<IEnumerable<SelectListItem>> GetAircraftsAsync(int? selectedId = null)
        {
            var list = await _aircraftRepository.GetAvailableAsync();
            return list.Select(a => new SelectListItem
            {
                Text = $"{a.Brand} {a.Model} (Cap: {a.TotalCapacity})",
                Value = a.Id.ToString(),
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            }).OrderBy(a => a.Text);
        }

        private async Task<IEnumerable<SelectListItem>> GetGatesAsync(int? selectedId = null)
        {
            var list = await _gateRepository.GetAvailableGatesAsync();
            var items = list.Select(g => new SelectListItem
            {
                Text = $"{g.GateNumber} - {g.Terminal}",
                Value = g.Id.ToString(),
                Selected = selectedId.HasValue && g.Id == selectedId.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Text = "(Sem Gate atribuído)", Value = "" });
            return items;
        }

        private IEnumerable<SelectListItem> GetStatuses(string? selectedStatus)
        {
            var statuses = new[]
            {
                ("Scheduled", "Previsto"),
                ("CheckIn", "Check-in"),
                ("Boarding", "A Embarcar"),
                ("Departed", "Partiu"),
                ("Delayed", "Atrasado"),
                ("Cancelled", "Cancelado")
            };

            return statuses.Select(s => new SelectListItem
            {
                Value = s.Item1,
                Text = s.Item2,
                Selected = s.Item1 == selectedStatus
            });
        }
    }
}