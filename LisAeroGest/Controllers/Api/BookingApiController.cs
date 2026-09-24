using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API para o fluxo de reserva e pagamento mobile.
    /// </summary>
    [Route("api/booking")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BookingApiController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IPayPalService _payPalService;

        public BookingApiController(
            ITicketRepository ticketRepository,
            ISeatRepository seatRepository,
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository,
            IPayPalService payPalService)
        {
            _ticketRepository = ticketRepository;
            _seatRepository = seatRepository;
            _flightRepository = flightRepository;
            _passengerRepository = passengerRepository;
            _payPalService = payPalService;
        }

        // ═══════════════════════════════════════════════════════════
        // 1. RESERVAR LUGAR
        // POST: /api/booking/reserve
        // ═══════════════════════════════════════════════════════════

        public class ReserveRequest
        {
            public int FlightId { get; set; }
            public int SeatId { get; set; }
            public bool ExtraLuggage { get; set; }
            public bool MealIncluded { get; set; }
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve([FromBody] ReserveRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não identificado." });

            var passenger = await _passengerRepository.GetByUserIdAsync(userId);
            if (passenger == null)
                return NotFound(new { message = "Perfil de passageiro não encontrado." });

            var flight = await _flightRepository.GetWithDetailsAsync(request.FlightId);
            if (flight == null || flight.WasDeleted)
                return NotFound(new { message = "Voo não encontrado." });

            var seat = await _seatRepository.GetByIdAsync(request.SeatId);
            if (seat == null || seat.WasDeleted)
                return NotFound(new { message = "Lugar não encontrado." });

            if (!seat.IsAvailable)
                return BadRequest(new { message = "Este lugar já foi reservado." });

            if (seat.FlightId != request.FlightId)
                return BadRequest(new { message = "O lugar não pertence a este voo." });

            // Calcular preço
            const decimal ExtraLuggageFee = 30m;
            const decimal MealFee = 15m;

            var totalPrice = flight.BasePrice
                           + seat.BasePrice
                           + (request.ExtraLuggage ? ExtraLuggageFee : 0m)
                           + (request.MealIncluded ? MealFee : 0m);

            // Bloquear lugar
            seat.IsAvailable = false;
            await _seatRepository.UpdateAsync(seat);

            // Criar ticket
            var ticket = new Ticket
            {
                FlightId = request.FlightId,
                SeatId = request.SeatId,
                PassengerId = passenger.Id,
                Status = "Reserved",
                TotalPrice = totalPrice,
                ExtraLuggage = request.ExtraLuggage,
                MealIncluded = request.MealIncluded,
                ReservationExpiresAt = DateTime.UtcNow.AddHours(2),
                PurchaseDate = DateTime.UtcNow,
                DownloadToken = Guid.NewGuid().ToString("N")
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveAsync();

            return Ok(new
            {
                ticketId = ticket.Id,
                flightNumber = flight.FlightNumber,
                seatCode = seat.Code,
                totalPrice = ticket.TotalPrice,
                expiresAt = ticket.ReservationExpiresAt,
                status = ticket.Status
            });
        }

        // ═══════════════════════════════════════════════════════════
        // 2. CRIAR ORDEM PAYPAL
        // POST: /api/booking/paypal/create
        // ═══════════════════════════════════════════════════════════

        public class CreateOrderRequest
        {
            public int TicketId { get; set; }
        }

        [HttpPost("paypal/create")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] CreateOrderRequest request)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);
            if (ticket == null)
                return NotFound(new { message = "Bilhete não encontrado." });

            if (ticket.Status != "Reserved")
                return BadRequest(new { message = "Esta reserva já foi processada ou expirou." });

            if (!ticket.IsReservationValid)
            {
                ticket.Status = "Expired";
                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveAsync();
                return BadRequest(new { message = "A reserva expirou." });
            }

            try
            {
                var orderId = await _payPalService.CreateOrderAsync(
                    ticket.TotalPrice,
                    "EUR",
                    ticket.Id.ToString()
                );

                return Ok(new { orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar ordem PayPal: " + ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // 3. CAPTURAR PAGAMENTO
        // POST: /api/booking/paypal/capture
        // ═══════════════════════════════════════════════════════════

        public class CaptureOrderRequest
        {
            public int TicketId { get; set; }
            public string OrderId { get; set; } = string.Empty;
        }

        [HttpPost("paypal/capture")]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] CaptureOrderRequest request)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);
            if (ticket == null)
                return NotFound(new { message = "Bilhete não encontrado." });

            if (ticket.Status != "Reserved")
                return BadRequest(new { message = "Esta reserva já foi processada." });

            try
            {
                var success = await _payPalService.CaptureOrderAsync(request.OrderId);
                if (!success)
                    return BadRequest(new { message = "Pagamento não confirmado pelo PayPal." });

                ticket.Status = "Paid";
                ticket.PurchaseDate = DateTime.UtcNow;
                ticket.ReservationExpiresAt = null;

                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveAsync();

                return Ok(new
                {
                    success = true,
                    ticketId = ticket.Id,
                    status = ticket.Status,
                    message = "Pagamento confirmado."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao capturar pagamento: " + ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // 4. CANCELAR RESERVA
        // POST: /api/booking/cancel/{ticketId}
        // ═══════════════════════════════════════════════════════════

        [HttpPost("cancel/{ticketId}")]
        public async Task<IActionResult> Cancel(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não identificado." });

            var passenger = await _passengerRepository.GetByUserIdAsync(userId);
            if (passenger == null)
                return NotFound(new { message = "Perfil não encontrado." });

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null || ticket.PassengerId != passenger.Id)
                return NotFound(new { message = "Bilhete não encontrado." });

            if (ticket.Status != "Reserved")
                return BadRequest(new { message = "Só reservas pendentes podem ser canceladas." });

            // Libertar lugar
            var seat = await _seatRepository.GetByIdAsync(ticket.SeatId);
            if (seat != null)
            {
                seat.IsAvailable = true;
                await _seatRepository.UpdateAsync(seat);
            }

            ticket.Status = "Cancelled";
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveAsync();

            return Ok(new { success = true, message = "Reserva cancelada." });
        }
    }
}