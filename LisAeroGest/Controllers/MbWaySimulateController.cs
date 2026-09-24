using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LisAeroGest.Data.Entities;
using System.Text.RegularExpressions;

namespace LisAeroGest.Controllers.Api
{
    public class MbWaySimulateRequest
    {
        public int TicketId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/payments/mbway")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MbWaySimulateController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly UserManager<User> _userManager;

        public MbWaySimulateController(
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            UserManager<User> userManager)
        {
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Simula um pagamento MB Way (projeto académico — não chama a SIBS).
        /// POST: api/payments/mbway/simulate
        /// </summary>
        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] MbWaySimulateRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var phone = (request.PhoneNumber ?? string.Empty).Replace(" ", "");
            if (!Regex.IsMatch(phone, @"^9\d{8}$"))
                return BadRequest(new { message = "Indica um telemóvel português válido (9 dígitos, a começar por 9)." });

            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger == null)
                return BadRequest(new { message = "Conta de passageiro não encontrada." });

            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId);
            if (ticket == null || ticket.WasDeleted)
                return NotFound(new { message = "Bilhete não encontrado." });

            if (ticket.PassengerId != passenger.Id)
                return Forbid();

            if (ticket.Status == "Paid" || ticket.Status == "CheckedIn")
                return Ok(new { success = true, message = "Este bilhete já está pago.", ticketId = ticket.Id, status = ticket.Status });

            if (ticket.Status == "Cancelled")
                return BadRequest(new { message = "Não é possível pagar um bilhete cancelado." });

            ticket.Status = "Paid";
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveAsync();

            return Ok(new
            {
                success = true,
                message = $"Pagamento MB Way simulado para {phone}.",
                ticketId = ticket.Id,
                status = ticket.Status
            });
        }
    }
}