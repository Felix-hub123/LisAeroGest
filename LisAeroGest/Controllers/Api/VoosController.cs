using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Inicializa o VoosController com as dependências necessárias.
        /// </summary>
        public VoosController(
            IFlightRepository flightRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository)
        {
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
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
    }
}
