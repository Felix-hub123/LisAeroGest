using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace LisAeroGest.Controllers.Api
{

    [ApiController]
    [Route("api/voos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VoosApiController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly UserManager<User> _userManager;

        public VoosApiController(
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IFlightRepository flightRepository,
            UserManager<User> userManager)
        {
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _flightRepository = flightRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtém os bilhetes do passageiro autenticado.
        /// GET: api/voos/my-tickets
        /// </summary>
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var passenger = await _passengerRepository
                .GetByUserIdAsync(user.Id);

            if (passenger == null)
                return Ok(Array.Empty<object>());

            var tickets = await _ticketRepository
                .GetByPassengerAsync(passenger.Id);

            var result = tickets.Select(t => new
            {
                id = t.Id,
                flightNumber = t.Flight?.FlightNumber ?? string.Empty,
                origin = t.Flight?.OriginAirport?.City
                         ?? t.Flight?.OriginAirport?.IATACode
                         ?? string.Empty,
                destination = t.Flight?.DestinationAirport?.City
                              ?? t.Flight?.DestinationAirport?.IATACode
                              ?? string.Empty,
                departureTime = t.Flight?.DepartureTime
                                 ?? DateTime.MinValue,
                seatCode = t.Seat?.Code ?? string.Empty,
                seatClass = t.Seat?.SeatClass ?? string.Empty,
                status = t.Status,
                totalPrice = t.TotalPrice,
                extraLuggage = t.ExtraLuggage,
                mealIncluded = t.MealIncluded,
                boardingPassId = (int?)null
            });

            return Ok(result);
        }


        /// <summary>
        /// Obtém os detalhes de um voo específico.
        /// GET: api/voos/{id}
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetails(int id)
        {
            var flight = await _flightRepository
                .GetWithDetailsAsync(id);

            if (flight == null || flight.WasDeleted)
            {
                return NotFound(new
                {
                    message = "Voo não encontrado."
                });
            }

            var durationMinutes = (int)(
                flight.ArrivalTime - flight.DepartureTime
            ).TotalMinutes;

            return Ok(new
            {
                id = flight.Id,
                flightNumber = flight.FlightNumber,
                airlineName = flight.Airline?.Name ?? string.Empty,
                airlineCode = flight.Airline?.IATACode ?? string.Empty,
                origin = flight.OriginAirport?.City
                         ?? flight.OriginAirport?.Name
                         ?? string.Empty,
                originCode = flight.OriginAirport?.IATACode
                             ?? string.Empty,
                destination = flight.DestinationAirport?.City
                              ?? flight.DestinationAirport?.Name
                              ?? string.Empty,
                destinationCode = flight.DestinationAirport?.IATACode
                                  ?? string.Empty,
                aircraftModel = flight.Aircraft == null
                    ? string.Empty
                    : $"{flight.Aircraft.Brand} {flight.Aircraft.Model}",
                gate = flight.Gate?.GateNumber ?? "TBA",
                departureTime = flight.DepartureTime,
                arrivalTime = flight.ArrivalTime,
                basePrice = flight.BasePrice,
                status = flight.Status,
                durationMinutes = durationMinutes > 0
                    ? durationMinutes
                    : 0
            });
        }


        /// <summary>
        /// Obtém os lugares disponíveis de um voo.
        /// GET: api/voos/{id}/seats
        /// </summary>
        [HttpGet("{id}/seats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeats(int id)
        {
            var flight = await _flightRepository
                .GetWithDetailsAsync(id);

            if (flight == null || flight.WasDeleted)
            {
                return NotFound(new
                {
                    message = "Voo não encontrado."
                });
            }
            var seats = flight.Seats?
             .Where(seat => !seat.WasDeleted && seat.IsAvailable)
             .Select(seat => new
             {
                 id = seat.Id,
                 code = seat.Code,
                 seatClass = seat.SeatClass,
                 basePrice = seat.BasePrice,
                 isAvailable = seat.IsAvailable
             })
             .OrderBy(seat => seat.seatClass)
             .ThenBy(seat => seat.code)
             .ToList();

            if (seats == null)
            {
                return Ok(Array.Empty<object>());
            }

            return Ok(seats);


        }
    }
}
