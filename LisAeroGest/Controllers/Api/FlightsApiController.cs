using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers.Api
{
    [ApiController]
    [Route("api/flights")]
    public class FlightsApiController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ISeatRepository _seatRepository; 

        public FlightsApiController(
            IFlightRepository flightRepository,
            ISeatRepository seatRepository)
        {
            _flightRepository = flightRepository;
            _seatRepository = seatRepository; 
        }

        /// <summary>
        /// Lista de voos de partida (agenda futura).
        /// GET: api/flights/departures
        /// </summary>
        [HttpGet("departures")]
        public async Task<IActionResult> GetDepartures()
        {
            var departures = await _flightRepository.GetAllQueryable()
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .Where(f => !f.WasDeleted)
                .Where(f => f.Status != "Cancelled")
                .Where(f => f.Status != "Arrived")
                .OrderBy(f => f.DepartureTime)
                .Select(f => new
                {
                    f.Id,
                    f.FlightNumber,
                    AirlineName = f.Airline != null ? f.Airline.Name : string.Empty,
                    AirlineCode = f.Airline != null ? f.Airline.IATACode : string.Empty,
                    Origin = f.OriginAirport != null ? f.OriginAirport.City : string.Empty,
                    Destination = f.DestinationAirport != null ? f.DestinationAirport.City : string.Empty,
                    DestinationCode = f.DestinationAirport != null ? f.DestinationAirport.IATACode : string.Empty,
                    Gate = f.Gate != null ? f.Gate.GateNumber : "TBD",
                    f.DepartureTime,
                    f.Status
                })
                .ToListAsync();

            return Ok(departures);
        }

        /// <summary>
        /// Lista de voos de chegada.
        /// GET: api/flights/arrivals
        /// </summary>
        [HttpGet("arrivals")]
        public async Task<IActionResult> GetArrivals()
        {
            var arrivals = await _flightRepository.GetAllQueryable()
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .Where(f => !f.WasDeleted)
                .Where(f => f.Status != "Cancelled")
                .OrderBy(f => f.ArrivalTime)
                .Select(f => new
                {
                    f.Id,
                    f.FlightNumber,
                    AirlineName = f.Airline != null ? f.Airline.Name : string.Empty,
                    AirlineCode = f.Airline != null ? f.Airline.IATACode : string.Empty,
                    Origin = f.OriginAirport != null ? f.OriginAirport.City : string.Empty,
                    OriginCode = f.OriginAirport != null ? f.OriginAirport.IATACode : string.Empty,
                    Destination = f.DestinationAirport != null ? f.DestinationAirport.City : string.Empty,
                    Gate = f.Gate != null ? f.Gate.GateNumber : "TBD",
                    f.ArrivalTime,
                    f.Status
                })
                .ToListAsync();

            return Ok(arrivals);
        }

        // ═══════════════════════════════════════════════════════
        // 🔥 NOVOS ENDPOINTS PARA O FLUXO DE COMPRA MOBILE
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Detalhes de um voo específico.
        /// GET: api/flights/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            var flight = await _flightRepository.GetAllQueryable()
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Gate)
                .Where(f => !f.WasDeleted)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
                return NotFound(new { message = "Voo não encontrado." });

            return Ok(new
            {
                flight.Id,
                flight.FlightNumber,
                AirlineName = flight.Airline?.Name ?? "",
                AirlineCode = flight.Airline?.IATACode ?? "",
                Origin = flight.OriginAirport?.City ?? "",
                OriginCode = flight.OriginAirport?.IATACode ?? "",
                Destination = flight.DestinationAirport?.City ?? "",
                DestinationCode = flight.DestinationAirport?.IATACode ?? "",
                AircraftModel = flight.Aircraft?.Model ?? "",
                Gate = flight.Gate?.GateNumber ?? "TBD",
                flight.DepartureTime,
                flight.ArrivalTime,
                flight.BasePrice,
                flight.Status,
                DurationMinutes = (int)(flight.ArrivalTime - flight.DepartureTime).TotalMinutes
            });
        }

        /// <summary>
        /// Lista os lugares disponíveis de um voo.
        /// GET: api/flights/{id}/seats
        /// </summary>
        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetFlightSeats(int id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null || flight.WasDeleted)
                return NotFound(new { message = "Voo não encontrado." });

            var seats = await _seatRepository.GetSeatsByFlightAsync(id);

            var result = seats
                .Where(s => !s.WasDeleted)
                .OrderBy(s => s.Code)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.SeatClass,
                    s.BasePrice,
                    s.IsAvailable
                })
                .ToList();

            return Ok(result);
        }
    }
}