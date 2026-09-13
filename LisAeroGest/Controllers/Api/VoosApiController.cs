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
        private readonly UserManager<User> _userManager;

        public VoosApiController(
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            UserManager<User> userManager)
        {
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userManager = userManager;
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger == null)
                return Ok(Array.Empty<object>());

            var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);

            var result = tickets.Select(t => new
            {
                id = t.Id,
                flightNumber = t.Flight?.FlightNumber ?? "",
                origin = t.Flight?.OriginAirport?.City ?? t.Flight?.OriginAirport?.IATACode ?? "",
                destination = t.Flight?.DestinationAirport?.City ?? t.Flight?.DestinationAirport?.IATACode ?? "",
                departureTime = t.Flight?.DepartureTime ?? DateTime.MinValue,
                seatCode = t.Seat?.Code ?? "",
                seatClass = t.Seat?.SeatClass ?? "",
                status = t.Status,
                totalPrice = t.TotalPrice,
                extraLuggage = t.ExtraLuggage,
                mealIncluded = t.MealIncluded,
                boardingPassId = (int?)null
            });

            return Ok(result);
        }
    }
}