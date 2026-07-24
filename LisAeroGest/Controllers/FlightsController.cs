using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de voos na plataforma LisAeroGest.
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
        private readonly IConverterHelper _converterHelper;

        public FlightsController(
            IFlightRepository flightRepository,
            IAirlineRepository airlineRepository,
            IAirportRepository airportRepository,
            IAircraftRepository aircraftRepository,
            IGateRepository gateRepository,
            IConverterHelper converterHelper)
        {
            _flightRepository = flightRepository;
            _airlineRepository = airlineRepository;
            _airportRepository = airportRepository;
            _aircraftRepository = aircraftRepository;
            _gateRepository = gateRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista os voos com suporte a filtros por companhia, origem, destino e estado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? airlineId, int? originId, int? destinationId, string? status)
        {
            // Obtém a consulta base de voos sem executar imediatamente na BD
            var query = _flightRepository.GetAllQueryable();

            // Aplicação dinâmica dos filtros selecionados pelo utilizador
            if (airlineId.HasValue) query = query.Where(f => f.AirlineId == airlineId.Value);
            if (originId.HasValue) query = query.Where(f => f.OriginAirportId == originId.Value);
            if (destinationId.HasValue) query = query.Where(f => f.DestinationAirportId == destinationId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(f => f.Status == status);

            // Ordena os voos por data de partida (mais recentes/futuros primeiro)
            var flights = await query.OrderByDescending(f => f.DepartureTime).ToListAsync();

            // Constrói a ViewModel com os dados da tabela e as dropdowns de filtro
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

            // Guarda a lista de aeroportos para o filtro de destino via ViewBag
            ViewBag.Destinations = _converterHelper.ToComboAirports(await _airportRepository.GetAllAsync(), destinationId);

            return View(model);
        }

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

        /// <summary>
        /// Apresenta o formulário para criação de um novo voo (Apenas Admin).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // Sugere horários padrão de partida e chegada
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
            // Validar se a partida não é no passado
            if (viewModel.DepartureTime <= DateTime.Now)
                ModelState.AddModelError("DepartureTime", "A hora de partida não pode ser no passado.");

            // Validar se a chegada é posterior à partida
            if (viewModel.ArrivalTime <= viewModel.DepartureTime)
                ModelState.AddModelError("ArrivalTime", "A hora de chegada tem de ser posterior à partida.");

            // Validar se a origem é diferente do destino
            if (viewModel.OriginAirportId == viewModel.DestinationAirportId)
                ModelState.AddModelError("DestinationAirportId", "O aeroporto de destino tem de ser diferente da origem.");

            // Validar disponibilidade da porta de embarque (Gate), se selecionada
            if (viewModel.GateId.HasValue && viewModel.GateId.Value > 0)
            {
                if (await _gateRepository.IsGateOccupiedAsync(viewModel.GateId.Value, viewModel.DepartureTime, viewModel.ArrivalTime))
                    ModelState.AddModelError("GateId", "O Gate selecionado já está ocupado por outro voo neste horário.");
            }

            // Se existirem erros de validação, recarrega as dropdowns e devolve a View
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel, viewModel.Status);
                return View(viewModel);
            }

            // Converter ViewModel em entidade Flight
            var flight = _converterHelper.ToFlight(viewModel, isEdit: false);
            var aircraft = await _aircraftRepository.GetWithSeatsAsync(viewModel.AircraftId);

            // Geração dos lugares do voo com base nos templates da aeronave ou na capacidade total
            if (aircraft?.Seats != null && aircraft.Seats.Any())
            {
                // Gera os lugares com base na estrutura de assentos predefinida da aeronave
                foreach (var seatTemplate in aircraft.Seats)
                {
                    flight.Seats.Add(new Seat
                    {
                        Code = seatTemplate.Code,
                        SeatClass = seatTemplate.SeatClass,
                        BasePrice = seatTemplate.SeatClass == "Business" ? viewModel.BasePrice * 1.5m : viewModel.BasePrice,
                        IsAvailable = true
                    });
                }
            }
            else
            {
                // Fallback: Gera os lugares sequencialmente a partir da capacidade total
                flight.Seats = _converterHelper.GenerateSeatsFromAircraftCapacity(aircraft, viewModel.BasePrice);
            }

            await _flightRepository.AddAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} criado com sucesso com {flight.Seats.Count} lugares gerados!";
            return RedirectToAction(nameof(Index));
        }

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
            // Validações de datas e aeroportos
            if (viewModel.ArrivalTime <= viewModel.DepartureTime)
                ModelState.AddModelError("ArrivalTime", "A chegada tem de ser depois da partida.");

            if (viewModel.OriginAirportId == viewModel.DestinationAirportId)
                ModelState.AddModelError("DestinationAirportId", "O destino tem de ser diferente da origem.");

            // Validar conflito de Gate excluindo o próprio voo atual
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

            var flight = _converterHelper.ToFlight(viewModel, isEdit: true);

            await _flightRepository.UpdateAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Altera o estado operacional do voo (ex: Programado, Check-in, Embarque, Atrasado, etc.).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null) return NotFound();

            // Lista de estados operacionais válidos no sistema
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
        /// Executa a remoção definitiva do voo.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(id);
            if (flight == null) return NotFound();

            // Regra de negócio: impede a eliminação de voos que já tenham bilhetes/lugares vendidos
            if (flight.Seats != null && flight.Seats.Any(s => !s.IsAvailable))
            {
                TempData["Error"] = "Não é possível eliminar este voo pois já existem bilhetes vendidos.";
                return RedirectToAction(nameof(Index));
            }

            await _flightRepository.DeleteAsync(flight);
            await _flightRepository.SaveAsync();

            TempData["Success"] = $"Voo {flight.FlightNumber} removido com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Método auxiliar para popular todas as listas pendentes (SelectLists) necessárias para as Views.
        /// </summary>
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