using LisAeroGest.Controllers.Api.Entities;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API para operações de funcionário (check-in presencial, pesquisa de passageiros).
    /// </summary>
    [Route("api/employee")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Employee,Admin")]
    public class EmployeeApiController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IBoardingPassRepository _boardingPassRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IGateRepository _gateRepository;

        public EmployeeApiController(
            ITicketRepository ticketRepository,
            IBoardingPassRepository boardingPassRepository,
            IPassengerRepository passengerRepository,
            IFlightRepository flightRepository,
            IGateRepository gateRepository)
        {
            _ticketRepository = ticketRepository;
            _boardingPassRepository = boardingPassRepository;
            _passengerRepository = passengerRepository;
            _flightRepository = flightRepository;
            _gateRepository = gateRepository;
        }





        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var flights = _ticketRepository
                .GetAllQueryable()
                .Include(t => t.Flight)
                .Where(t =>
                    !t.WasDeleted &&
                    t.Flight != null &&
                    t.Flight.DepartureTime >= today &&
                    t.Flight.DepartureTime < tomorrow);

            var totalPassengers = await flights
                .Select(t => t.PassengerId)
                .Distinct()
                .CountAsync();

            var totalTickets =
                await flights.CountAsync();

            var pendingCheckIns =
                await flights.CountAsync(t =>
                    t.Status == "Paid");

            var checkedIn =
                await flights.CountAsync(t =>
                    t.Status == "CheckedIn");

            var flightCount =
                await flights
                    .Select(t => t.FlightId)
                    .Distinct()
                    .CountAsync();

            return Ok(new
            {
                flightCount,
                totalPassengers,
                totalTickets,
                pendingCheckIns,
                checkedIn
            });
        }

        // ═══════════════════════════════════════════════════════════
        // 1. PESQUISAR BILHETES PARA CHECK-IN
        // GET: /api/employee/checkin/search?query=...
        // ═══════════════════════════════════════════════════════════

        [HttpGet("checkin/search")]
        public async Task<IActionResult> SearchTickets([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(Array.Empty<object>());

            var term = query.Trim();

            // Procura por nome, email, documento, telefone, voo ou ID do bilhete
            var tickets = await _ticketRepository.GetAllQueryable()
                .Include(t => t.Passenger)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.Gate)
                .Include(t => t.Seat)
                .Where(t => !t.WasDeleted
                    && t.Status == "Paid")   // Só bilhetes pagos podem fazer check-in
                .Where(t =>
                    t.Passenger != null &&
                    (
                        EF.Functions.Like(t.Passenger.FirstName, $"%{term}%") ||
                        EF.Functions.Like(t.Passenger.LastName, $"%{term}%") ||
                        EF.Functions.Like(t.Passenger.Email, $"%{term}%") ||
                        EF.Functions.Like(t.Passenger.DocumentNumber, $"%{term}%") ||
                        (t.Passenger.PhoneNumber != null &&
                         EF.Functions.Like(t.Passenger.PhoneNumber, $"%{term}%")) ||
                        (t.Flight != null &&
                         EF.Functions.Like(t.Flight.FlightNumber, $"%{term}%"))
                    )
                )
                .OrderBy(t => t.Flight!.DepartureTime)
                .Take(20)
                .Select(t => new
                {
                    ticketId = t.Id,
                    passengerName = t.Passenger!.FirstName + " " + t.Passenger.LastName,
                    email = t.Passenger.Email,
                    documentNumber = t.Passenger.DocumentNumber,
                    flightNumber = t.Flight != null ? t.Flight.FlightNumber : "",
                    departureTime = t.Flight != null ? t.Flight.DepartureTime : DateTime.MinValue,
                    status = t.Status,
                    gate = t.Flight != null && t.Flight.Gate != null
                        ? t.Flight.Gate.GateNumber
                        : "TBA",
                    seat = t.Seat != null ? t.Seat.Code : null,
                    canCheckIn = true
                })
                .ToListAsync();

            return Ok(tickets);
        }

        // ═══════════════════════════════════════════════════════════
        // 2. FAZER CHECK-IN PRESENCIAL
        // POST: /api/employee/checkin
        // ═══════════════════════════════════════════════════════════

        public class EmployeeCheckInRequest
        {
            public int TicketId { get; set; }
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> DoEmployeeCheckIn([FromBody] EmployeeCheckInRequest request)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);

            if (ticket == null)
                return NotFound(new { success = false, errorMessage = "Bilhete não encontrado." });

            if (ticket.Status != "Paid")
                return BadRequest(new { success = false, errorMessage = "Este bilhete não está pago ou já tem check-in." });

            if (ticket.Flight == null)
                return BadRequest(new { success = false, errorMessage = "Dados do voo indisponíveis." });

            if (ticket.Flight.Status == "Cancelled")
                return BadRequest(new { success = false, errorMessage = "Voo cancelado." });

            if (ticket.Flight.Status == "Departed")
                return BadRequest(new { success = false, errorMessage = "O voo já partiu." });

            // Verificar se já existe boarding pass (evitar double check-in)
            var existing = await _boardingPassRepository.GetByTicketIdAsync(ticket.Id);
            if (existing != null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Este bilhete já tem check-in feito.",
                    passengerName = $"{ticket.Passenger?.FirstName} {ticket.Passenger?.LastName}".Trim(),
                    flightNumber = ticket.Flight.FlightNumber,
                    gate = existing.Gate,
                    sequenceNumber = existing.SequenceNumber,
                    qrData = existing.QRCode
                });
            }

            // Criar boarding pass
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

            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            await _boardingPassRepository.SaveAsync();

            return Ok(new
            {
                success = true,
                message = "Check-in realizado com sucesso.",
                passengerName = $"{ticket.Passenger?.FirstName} {ticket.Passenger?.LastName}".Trim(),
                flightNumber = ticket.Flight.FlightNumber,
                gate = gateNumber,
                sequenceNumber = boardingPass.SequenceNumber,
                qrData = boardingPass.QRCode
            });
        }



        [HttpPost("checkin/by-name")]
        public async Task<IActionResult> CheckInByPassengerData(
                 [FromBody] EmployeeNameCheckInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PassengerName) &&
                string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage =
                        "Indique o nome do passageiro ou o número do documento."
                });
            }

            var query = _ticketRepository
       .GetAllQueryable()
       .Include(t => t.Passenger)
       .Include(t => t.Flight)
           .ThenInclude(f => f!.Gate)
       .Include(t => t.Seat)
       .Where(t =>
           !t.WasDeleted &&
           t.Status == "Paid" &&
           t.Passenger != null &&
           t.Flight != null);

            // Search by document number
            if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                var documentNumber = request.DocumentNumber.Trim();

                query = query.Where(t =>
                    t.Passenger!.DocumentNumber == documentNumber);
            }
            else
            {
                // Search by passenger name
                var passengerName = request.PassengerName.Trim();

                var nameParts = passengerName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

                if (nameParts.Length == 1)
                {
                    query = query.Where(t =>
                        EF.Functions.Like(
                            t.Passenger!.FirstName!,
                            $"%{passengerName}%") ||

                        EF.Functions.Like(
                            t.Passenger!.LastName!,
                            $"%{passengerName}%"));
                }
                else
                {
                    var firstName = nameParts[0];
                    var lastName = nameParts[^1];

                    query = query.Where(t =>
                        EF.Functions.Like(
                            t.Passenger!.FirstName!,
                            $"%{firstName}%") &&

                        EF.Functions.Like(
                            t.Passenger!.LastName!,
                            $"%{lastName}%"));
                }
            }

            // If the flight number was provided,
            // use it to narrow the search.
            if (!string.IsNullOrWhiteSpace(request.FlightNumber))
            {
                var flightNumber = request.FlightNumber.Trim();

                query = query.Where(t =>
                    t.Flight!.FlightNumber == flightNumber);
            }

            var matches = await query
                .OrderBy(t => t.Flight!.DepartureTime)
                .Take(10)
                .ToListAsync();

            if (matches.Count == 0)
            {
                return NotFound(new
                {
                    success = false,
                    errorMessage =
                        "Não foi encontrado um bilhete pago para estes dados."
                });
            }

            // If multiple tickets have the same passenger name,
            // request the flight number to avoid checking in
            // the wrong passenger.
            if (matches.Count > 1 &&
                string.IsNullOrWhiteSpace(request.FlightNumber) &&
                string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                return Conflict(new
                {
                    success = false,

                    errorMessage =
                        "Foram encontrados vários bilhetes. " +
                        "Indique também o número do voo.",

                    matches = matches.Select(t => new
                    {
                        ticketId = t.Id,

                        passengerName =
                            $"{t.Passenger!.FirstName} " +
                            $"{t.Passenger.LastName}".Trim(),

                        flightNumber =
                            t.Flight!.FlightNumber,

                        departureTime =
                            t.Flight.DepartureTime,

                        seat =
                            t.Seat?.Code
                    })
                });
            }

            return await PerformEmployeeCheckIn(matches[0]);


        }



        private async Task<IActionResult> PerformEmployeeCheckIn(Ticket ticket)
        {
            if (ticket.Status != "Paid")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage =
                        "Este bilhete não está pago ou já tem check-in."
                });
            }

            if (ticket.Flight == null)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Dados do voo indisponíveis."
                });
            }

            if (ticket.Flight.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Voo cancelado."
                });
            }

            if (ticket.Flight.Status == "Departed")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "O voo já partiu."
                });
            }

            // Prevent duplicate check-in
            var existingBoardingPass =
                await _boardingPassRepository.GetByTicketIdAsync(ticket.Id);

            if (existingBoardingPass != null)
            {
                return Ok(new
                {
                    success = true,

                    message =
                        "Este bilhete já tem check-in feito.",

                    passengerName =
                        $"{ticket.Passenger?.FirstName} " +
                        $"{ticket.Passenger?.LastName}".Trim(),

                    flightNumber =
                        ticket.Flight.FlightNumber,

                    gate =
                        existingBoardingPass.Gate,

                    sequenceNumber =
                        existingBoardingPass.SequenceNumber,

                    qrData =
                        existingBoardingPass.QRCode
                });
            }

            var gateNumber =
                ticket.Flight.Gate?.GateNumber ?? "TBA";

            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,

                IssuedAt = DateTime.UtcNow,

                Gate = gateNumber,

                SequenceNumber =
                    await _boardingPassRepository
                        .GetNextSequenceNumberAsync(ticket.FlightId),

                QRCode =
                    $"BOARDING|{ticket.Id}|" +
                    $"{ticket.Flight.FlightNumber}|" +
                    $"{gateNumber}"
            };

            await _boardingPassRepository.AddAsync(boardingPass);

            ticket.Status = "CheckedIn";

            await _ticketRepository.UpdateAsync(ticket);

            await _boardingPassRepository.SaveAsync();

            return Ok(new
            {
                success = true,

                message =
                    "Check-in realizado com sucesso.",

                passengerName =
                    $"{ticket.Passenger?.FirstName} " +
                    $"{ticket.Passenger?.LastName}".Trim(),

                flightNumber =
                    ticket.Flight.FlightNumber,

                gate =
                    gateNumber,

                sequenceNumber =
                    boardingPass.SequenceNumber,

                qrData =
                    boardingPass.QRCode
            });
        }


        // ═══════════════════════════════════════════════════════════
        // GESTÃO OPERACIONAL DE PORTAS
        // ═══════════════════════════════════════════════════════════

        public class ChangeGateRequest
        {
            public int GateId { get; set; }
        }


        /// <summary>
        /// Lista todas as portas que podem ser utilizadas.
        /// GET: /api/employee/gates
        /// </summary>
        [HttpGet("gates")]
        public async Task<IActionResult> GetGates()
        {
            var gates = await _gateRepository
                .GetAllQueryable()
                .Where(g => !g.WasDeleted)
                .OrderBy(g => g.Terminal)
                .ThenBy(g => g.GateNumber)
                .Select(g => new
                {
                    g.Id,
                    g.GateNumber,
                    g.Terminal,
                    g.Status
                })
                .ToListAsync();

            return Ok(gates);
        }


        /// <summary>
        /// Altera a porta atribuída a um voo.
        /// PUT: /api/employee/flights/{flightId}/gate
        /// </summary>
        [HttpPut("flights/{flightId:int}/gate")]
        public async Task<IActionResult> ChangeFlightGate(
            int flightId,
            [FromBody] ChangeGateRequest request)
        {
            if (request.GateId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Selecione uma porta válida."
                });
            }

            // Procurar voo
            var flight = await _flightRepository
                .GetAllQueryable()
                .Include(f => f.Gate)
                .FirstOrDefaultAsync(f =>
                    f.Id == flightId &&
                    !f.WasDeleted);

            if (flight == null)
            {
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Voo não encontrado."
                });
            }

            // Não permitir alterações em voos terminados
            if (flight.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage =
                        "Não é possível alterar a porta de um voo cancelado."
                });
            }

            if (flight.Status == "Departed" ||
                flight.Status == "Arrived")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage =
                        "Não é possível alterar a porta de um voo já concluído."
                });
            }

            // Procurar nova porta
            var gate = await _gateRepository
                .GetAllQueryable()
                .FirstOrDefaultAsync(g =>
                    g.Id == request.GateId &&
                    !g.WasDeleted);

            if (gate == null)
            {
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Porta não encontrada."
                });
            }

            // Porta em manutenção
            if (gate.Status == "Maintenance")
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage =
                        "Esta porta encontra-se em manutenção."
                });
            }

            // Se já é a mesma porta, não há nada para alterar
            if (flight.GateId == gate.Id)
            {
                return Ok(new
                {
                    success = true,
                    message = "O voo já está atribuído a esta porta.",
                    flightId = flight.Id,
                    flightNumber = flight.FlightNumber,
                    gateId = gate.Id,
                    gateNumber = gate.GateNumber
                });
            }

            // Verificar conflito operacional
            var occupied =
                await _gateRepository.IsGateOccupiedAsync(
                    gate.Id,
                    flight.DepartureTime,
                    flight.ArrivalTime,
                    flight.Id);

            if (occupied)
            {
                return Conflict(new
                {
                    success = false,
                    errorMessage =
                        $"A porta {gate.GateNumber} já está ocupada " +
                        "por outro voo neste período."
                });
            }

            var previousGate =
                flight.Gate?.GateNumber ?? "Sem porta";

            // Alterar porta
            flight.GateId = gate.Id;
            flight.Gate = gate;

            await _flightRepository.UpdateAsync(flight);

            return Ok(new
            {
                success = true,
                message =
                    $"Porta alterada de {previousGate} para {gate.GateNumber}.",

                flightId = flight.Id,
                flightNumber = flight.FlightNumber,

                previousGate,

                gateId = gate.Id,
                gateNumber = gate.GateNumber,

                terminal = gate.Terminal
            });
        }


    }
}