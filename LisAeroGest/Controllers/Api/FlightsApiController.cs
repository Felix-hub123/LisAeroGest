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

        public FlightsApiController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        /// <summary>
        /// Obtém a lista de voos de partida para a App Móvel .NET MAUI.
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
                    GateNumber = f.Gate != null ? f.Gate.GateNumber : "TBD",
                    f.DepartureTime,
                    f.Status
                })
                .ToListAsync();

            return Ok(departures);
        }

    }
}
