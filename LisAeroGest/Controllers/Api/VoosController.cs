using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API REST para consulta de voos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VoosController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IBoardingPassRepository _boardingPassRepository;

        /// <summary>
        /// Inicializa o VoosController com as dependências necessárias.
        /// </summary>
        public VoosController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IBoardingPassRepository boardingPassRepository)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _boardingPassRepository = boardingPassRepository;
        }

        /// <summary>
        /// Obtém a lista de voos disponíveis (com filtros opcionais).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetVoos([FromQuery] string? origem, [FromQuery] string? destino, [FromQuery] DateTime? data)
        {
            var voos = await _flightRepository.GetAvailableFlightsAsync(origem, destino, data);
            var result = voos.Select(f => new
            {
                f.Id,
                f.FlightNumber,
                Companhia = f.Airline?.Name,
                Origem = f.OriginAirport?.IATACode,
                Destino = f.DestinationAirport?.IATACode,
                Partida = f.DepartureTime,
                Chegada = f.ArrivalTime,
                f.Status,
                Preco = f.BasePrice,
                Gate = f.Gate?.GateNumber
            });

            return Ok(result);
        }

        /// <summary>
        /// Obtém os detalhes de um voo específico.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVoo(int id)
        {
            var voo = await _flightRepository.GetWithDetailsAsync(id);
            if (voo == null) return NotFound();

            return Ok(new
            {
                voo.Id,
                voo.FlightNumber,
                Companhia = voo.Airline?.Name,
                Origem = voo.OriginAirport?.IATACode,
                Destino = voo.DestinationAirport?.IATACode,
                Partida = voo.DepartureTime,
                Chegada = voo.ArrivalTime,
                voo.Status,
                Preco = voo.BasePrice,
                Gate = voo.Gate?.GateNumber,
                Aeronave = $"{voo.Aircraft?.Brand} {voo.Aircraft?.Model}"
            });
        }

        /// <summary>
        /// Gets the authenticated passenger's tickets, including check-in status.
        /// GET: api/voos/my-tickets
        /// </summary>
        [HttpGet("my-tickets")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not identified in token." });

            var passenger = await _passengerRepository.GetByUserIdAsync(userId);
            if (passenger == null)
                return NotFound(new { message = "Passenger profile not found for this user." });

            var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);

            var result = new List<object>();

            foreach (var t in tickets)
            {
                int? boardingPassId = null;
                if (t.HasCheckedIn)
                {
                    var bp = await _boardingPassRepository.GetByTicketIdAsync(t.Id);
                    boardingPassId = bp?.Id;
                }

                result.Add(new
                {
                    t.Id,
                    FlightNumber = t.Flight?.FlightNumber ?? string.Empty,
                    Origin = t.Flight?.OriginAirport?.City ?? string.Empty,
                    Destination = t.Flight?.DestinationAirport?.City ?? string.Empty,
                    DepartureTime = t.Flight?.DepartureTime ?? DateTime.MinValue,
                    SeatCode = t.Seat?.Code ?? string.Empty,
                    SeatClass = t.Seat?.SeatClass ?? string.Empty,
                    t.Status,
                    t.TotalPrice,
                    ExtraLuggage = t.ExtraLuggage,
                    MealIncluded = t.MealIncluded,
                    BoardingPassId = boardingPassId
                });
            }

            return Ok(result);
        }
    }
}
