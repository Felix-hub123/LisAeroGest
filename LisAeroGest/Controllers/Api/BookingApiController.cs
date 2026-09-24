using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API para o fluxo de reserva e pagamento da aplicação mobile.
    /// O pagamento MB WAY é apenas uma simulação académica.
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

        public BookingApiController(
            ITicketRepository ticketRepository,
            ISeatRepository seatRepository,
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository)
        {
            _ticketRepository = ticketRepository;
            _seatRepository = seatRepository;
            _flightRepository = flightRepository;
            _passengerRepository = passengerRepository;
        }

        // ═══════════════════════════════════════════════════════════
        // 1. RESERVE SEAT
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
        public async Task<IActionResult> Reserve(
            [FromBody] ReserveRequest request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "Utilizador não identificado."
                });
            }

            var passenger =
                await _passengerRepository.GetByUserIdAsync(userId);

            if (passenger == null)
            {
                return NotFound(new
                {
                    message = "Perfil de passageiro não encontrado."
                });
            }

            var flight =
                await _flightRepository
                    .GetWithDetailsAsync(request.FlightId);

            if (flight == null || flight.WasDeleted)
            {
                return NotFound(new
                {
                    message = "Voo não encontrado."
                });
            }

            var seat =
                await _seatRepository.GetByIdAsync(request.SeatId);

            if (seat == null || seat.WasDeleted)
            {
                return NotFound(new
                {
                    message = "Lugar não encontrado."
                });
            }

            if (!seat.IsAvailable)
            {
                return BadRequest(new
                {
                    message = "Este lugar já foi reservado."
                });
            }

            if (seat.FlightId != request.FlightId)
            {
                return BadRequest(new
                {
                    message = "O lugar não pertence a este voo."
                });
            }

            // Additional services
            const decimal ExtraLuggageFee = 30m;
            const decimal MealFee = 15m;

            var totalPrice =
                flight.BasePrice
                + seat.BasePrice
                + (request.ExtraLuggage
                    ? ExtraLuggageFee
                    : 0m)
                + (request.MealIncluded
                    ? MealFee
                    : 0m);

            // Lock the selected seat
            seat.IsAvailable = false;

            await _seatRepository.UpdateAsync(seat);

            // Create reservation
            var ticket = new Ticket
            {
                FlightId = request.FlightId,
                SeatId = request.SeatId,
                PassengerId = passenger.Id,

                Status = "Reserved",

                TotalPrice = totalPrice,

                ExtraLuggage = request.ExtraLuggage,
                MealIncluded = request.MealIncluded,

                ReservationExpiresAt =
                    DateTime.UtcNow.AddHours(2),

                PurchaseDate = DateTime.UtcNow,

                DownloadToken =
                    Guid.NewGuid().ToString("N")
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
        // 2. SIMULATED MB WAY PAYMENT
        // POST: /api/booking/mbway/pay
        // ═══════════════════════════════════════════════════════════

        public class MbWayPaymentRequest
        {
            public int TicketId { get; set; }

            public string PhoneNumber { get; set; } =
                string.Empty;
        }

        [HttpPost("mbway/pay")]
        public async Task<IActionResult> PayWithMbWay(
            [FromBody] MbWayPaymentRequest request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "Utilizador não identificado."
                });
            }

            // Get authenticated passenger
            var passenger =
                await _passengerRepository.GetByUserIdAsync(userId);

            if (passenger == null)
            {
                return NotFound(new
                {
                    message = "Perfil de passageiro não encontrado."
                });
            }

            // Validate phone number
            var phoneNumber =
                NormalizePhoneNumber(request.PhoneNumber);

            if (!IsValidPortuguesePhoneNumber(phoneNumber))
            {
                return BadRequest(new
                {
                    message =
                        "Introduza um número de telemóvel português válido."
                });
            }

            // Get reservation
            var ticket =
                await _ticketRepository
                    .GetTicketWithDetailsAsync(request.TicketId);

            if (ticket == null ||
                ticket.PassengerId != passenger.Id)
            {
                return NotFound(new
                {
                    message = "Reserva não encontrada."
                });
            }

            if (ticket.Status != "Reserved")
            {
                return BadRequest(new
                {
                    message =
                        "Esta reserva já foi processada."
                });
            }

            // Check reservation expiration
            if (!ticket.IsReservationValid)
            {
                ticket.Status = "Expired";

                // Release seat when reservation expires
                var expiredSeat =
                    await _seatRepository
                        .GetByIdAsync(ticket.SeatId);

                if (expiredSeat != null)
                {
                    expiredSeat.IsAvailable = true;

                    await _seatRepository
                        .UpdateAsync(expiredSeat);
                }

                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveAsync();

                return BadRequest(new
                {
                    message =
                        "A reserva expirou. Selecione novamente o lugar."
                });
            }

            // ═══════════════════════════════════════════════════════
            // MB WAY SIMULATION
            //
            // No real MB WAY transaction takes place here.
            // For academic demonstration purposes, a valid request
            // is considered an approved payment.
            // ═══════════════════════════════════════════════════════

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

                amount = ticket.TotalPrice,

                phoneNumber =
                    MaskPhoneNumber(phoneNumber),

                transactionId =
                    $"SIM-{Guid.NewGuid():N}"
                        .ToUpperInvariant()[..16],

                paidAt = DateTime.UtcNow,

                message =
                    "Pagamento MB WAY simulado com sucesso."
            });
        }

        // ═══════════════════════════════════════════════════════════
        // 3. CANCEL RESERVATION
        // POST: /api/booking/cancel/{ticketId}
        // ═══════════════════════════════════════════════════════════

        [HttpPost("cancel/{ticketId}")]
        public async Task<IActionResult> Cancel(int ticketId)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "Utilizador não identificado."
                });
            }

            var passenger =
                await _passengerRepository.GetByUserIdAsync(userId);

            if (passenger == null)
            {
                return NotFound(new
                {
                    message = "Perfil não encontrado."
                });
            }

            var ticket =
                await _ticketRepository
                    .GetTicketWithDetailsAsync(ticketId);

            if (ticket == null ||
                ticket.PassengerId != passenger.Id)
            {
                return NotFound(new
                {
                    message = "Bilhete não encontrado."
                });
            }

            if (ticket.Status != "Reserved")
            {
                return BadRequest(new
                {
                    message =
                        "Só reservas pendentes podem ser canceladas."
                });
            }

            // Release seat
            var seat =
                await _seatRepository
                    .GetByIdAsync(ticket.SeatId);

            if (seat != null)
            {
                seat.IsAvailable = true;

                await _seatRepository.UpdateAsync(seat);
            }

            ticket.Status = "Cancelled";

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveAsync();

            return Ok(new
            {
                success = true,
                message = "Reserva cancelada."
            });
        }

        // ═══════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════

        private static string NormalizePhoneNumber(
            string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var normalized = phoneNumber
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            if (normalized.StartsWith("+351"))
            {
                normalized = normalized[4..];
            }
            else if (normalized.StartsWith("00351"))
            {
                normalized = normalized[5..];
            }

            return normalized;
        }

        private static bool IsValidPortuguesePhoneNumber(
            string phoneNumber)
        {
            if (phoneNumber.Length != 9)
                return false;

            if (!phoneNumber.All(char.IsDigit))
                return false;

            return phoneNumber.StartsWith("9");
        }

        private static string MaskPhoneNumber(
            string phoneNumber)
        {
            if (phoneNumber.Length != 9)
                return phoneNumber;

            return
                $"{phoneNumber[..3]} *** {phoneNumber[^3..]}";
        }
    }
}