using LisAeroGest.Controllers.Api.Entities;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CheckInApiController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IBoardingPassRepository _boardingPassRepository;

        public CheckInApiController(
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IBoardingPassRepository boardingPassRepository)
        {
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _boardingPassRepository = boardingPassRepository;
        }

     

        /// <summary>
        /// Realiza o check-in de um bilhete a partir da app mobile e devolve os dados do cartão de embarque.
        /// POST: api/checkin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DoCheckIn([FromBody] CheckInRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not identified in token." });

            var passenger = await _passengerRepository.GetByUserIdAsync(userId);
            if (passenger == null)
                return NotFound(new { message = "Passenger profile not found." });

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
                return NotFound(new { message = "Ticket not found." });

            if (ticket.Status != "Paid")
                return BadRequest(new { message = "Only paid tickets are eligible for check-in." });

            if (ticket.Flight == null)
                return BadRequest(new { message = "Flight data unavailable for this ticket." });

            if (ticket.Flight.Status == "Cancelled")
                return BadRequest(new { message = "Cannot check in: flight is cancelled." });

            var now = DateTime.UtcNow;
            var checkInOpensAt = ticket.Flight.DepartureTime.AddHours(-48);
            var checkInClosesAt = ticket.Flight.DepartureTime.AddHours(-1);

            if (now < checkInOpensAt)
                return BadRequest(new { message = $"Check-in only opens at {checkInOpensAt:dd/MM HH:mm} (UTC)." });

            if (now > checkInClosesAt)
                return BadRequest(new { message = "Check-in window has closed for this flight." });

            ticket.Status = "CheckedIn";
            await _ticketRepository.UpdateAsync(ticket);

            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,
                IssuedAt = DateTime.UtcNow,
                Gate = ticket.Flight.Gate?.GateNumber ?? "TBA",
                SequenceNumber = await _boardingPassRepository.GetNextSequenceNumberAsync(ticket.FlightId),
                QRCode = $"BOARDING|{ticket.Id}|{ticket.Flight.FlightNumber}|{ticket.Flight.Gate?.GateNumber ?? "TBA"}"
            };

            await _boardingPassRepository.AddAsync(boardingPass);
            await _boardingPassRepository.SaveAsync();

            return Ok(new
            {
                boardingPass.Id,
                boardingPass.TicketId,
                FlightNumber = ticket.Flight.FlightNumber,
                boardingPass.Gate,
                boardingPass.SequenceNumber,
                boardingPass.IssuedAt,
                QRData = boardingPass.QRCode
            });
        }
    }
}
