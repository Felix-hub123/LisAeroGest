using LisAeroGest.Controllers.Api.Entities;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    [Route("api/checkin")]
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

     

      

        [HttpPost("validate")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> ValidateBoardingPass(
             [FromBody] ValidateBoardingPassRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QRData))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "QR Code inválido."
                });
            }

            // Formato:
            // BOARDING|TicketId|FlightNumber|Gate
            var parts = request.QRData.Split('|');

            if (parts.Length < 4 || parts[0] != "BOARDING")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "QR Code não reconhecido."
                });
            }

            if (!int.TryParse(parts[1], out var ticketId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bilhete inválido."
                });
            }

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Bilhete não encontrado."
                });
            }

            if (ticket.Status != "CheckedIn")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Este bilhete não tem check-in válido."
                });
            }

            if (ticket.Flight == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Voo associado ao bilhete não encontrado."
                });
            }

            // Confirma que os dados principais do QR correspondem ao bilhete
            var qrFlightNumber = parts[2];
            var qrGate = parts[3];

            if (ticket.Flight.FlightNumber != qrFlightNumber ||
                (ticket.Flight.Gate?.GateNumber ?? "TBA") != qrGate)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Os dados do QR Code não correspondem ao bilhete."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Bilhete válido.",
                ticketId = ticket.Id,
                flightNumber = ticket.Flight.FlightNumber,
                gate = ticket.Flight.Gate?.GateNumber ?? "TBA",
                status = ticket.Status
            });
        }





        /// <summary>
        /// Realiza o check-in de um bilhete a partir da app mobile
        /// e devolve os dados do cartão de embarque.
        /// Se já existir um cartão de embarque, devolve o existente.
        /// POST: api/checkin
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DoCheckIn([FromBody] CheckInRequest request)
        {
            // 1. Obter o utilizador autenticado através do token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "User not identified in token."
                });
            }

            // 2. Procurar o perfil do passageiro
            var passenger =
                await _passengerRepository.GetByUserIdAsync(userId);

            if (passenger == null)
            {
                return NotFound(new
                {
                    message = "Passenger profile not found."
                });
            }

            // 3. Procurar o bilhete
            var ticket =
                await _ticketRepository.GetTicketWithDetailsAsync(
                    request.TicketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }

            // 4. Verificar se já existe um cartão de embarque
            var existingBoardingPass =
                await _boardingPassRepository.GetByTicketIdAsync(ticket.Id);

            // Se já existir, devolvemos o mesmo cartão.
            // Isto impede a criação de dois BoardingPass para o mesmo bilhete.
            if (existingBoardingPass != null)
            {
                return Ok(new
                {
                    existingBoardingPass.Id,
                    existingBoardingPass.TicketId,

                    FlightNumber =
                        ticket.Flight?.FlightNumber ?? string.Empty,

                    existingBoardingPass.Gate,
                    existingBoardingPass.SequenceNumber,
                    existingBoardingPass.IssuedAt,

                    QRData = existingBoardingPass.QRCode
                });
            }

            if (ticket.Status != "Paid")
            {
                return BadRequest(new
                {
                    message =
                        $"Check-in recusado. Ticket {ticket.Id} está com Status='{ticket.Status}'."
                });
            }

            // 6. Confirmar que o voo existe
            if (ticket.Flight == null)
            {
                return BadRequest(new
                {
                    message =
                        "Flight data unavailable for this ticket."
                });
            }

            // 7. Não permitir check-in de um voo cancelado
            if (ticket.Flight.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message =
                        "Cannot check in: flight is cancelled."
                });
            }

            // 8. Validar a janela de check-in
            var now = DateTime.UtcNow;

            var checkInOpensAt =
                ticket.Flight.DepartureTime.AddHours(-48);

            var checkInClosesAt =
                ticket.Flight.DepartureTime.AddHours(-1);

            if (now < checkInOpensAt)
            {
                return BadRequest(new
                {
                    message =
                        $"Check-in only opens at " +
                        $"{checkInOpensAt:dd/MM HH:mm} (UTC)."
                });
            }

            if (now > checkInClosesAt)
            {
                return BadRequest(new
                {
                    message =
                        "Check-in window has closed for this flight."
                });
            }

            // 9. Alterar o estado do bilhete para CheckedIn
            ticket.Status = "CheckedIn";

            await _ticketRepository.UpdateAsync(ticket);

            // 10. Obter o próximo número de sequência
            var sequenceNumber =
                await _boardingPassRepository
                    .GetNextSequenceNumberAsync(ticket.FlightId);

            // 11. Obter a porta de embarque
            var gate =
                ticket.Flight.Gate?.GateNumber ?? "TBA";

            // 12. Criar o cartão de embarque
            var boardingPass = new BoardingPass
            {
                TicketId = ticket.Id,

                IssuedAt = DateTime.UtcNow,

                Gate = gate,

                SequenceNumber = sequenceNumber,

                QRCode =
                    $"BOARDING|" +
                    $"{ticket.Id}|" +
                    $"{ticket.Flight.FlightNumber}|" +
                    $"{gate}"
            };

            // 13. Guardar o cartão de embarque na base de dados
            await _boardingPassRepository.AddAsync(boardingPass);

            await _boardingPassRepository.SaveAsync();

            // 14. Devolver os dados do cartão para a aplicação mobile
            return Ok(new
            {
                boardingPass.Id,

                boardingPass.TicketId,

                FlightNumber =
                    ticket.Flight.FlightNumber,

                boardingPass.Gate,

                boardingPass.SequenceNumber,

                boardingPass.IssuedAt,

                QRData =
                    boardingPass.QRCode
            });
        }


        /// <summary>
        /// Obtém o cartão de embarque de um bilhete que pertence
        /// ao passageiro autenticado.
        /// GET: api/checkin/{ticketId}
        /// </summary>
        [HttpGet("{ticketId:int}")]
        public async Task<IActionResult> GetBoardingPass(int ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "User not identified in token."
                });
            }

            var passenger = await _passengerRepository.GetByUserIdAsync(userId);

            if (passenger == null)
            {
                return NotFound(new
                {
                    message = "Passenger profile not found."
                });
            }

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null || ticket.PassengerId != passenger.Id)
            {
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }

            var boardingPass =
                await _boardingPassRepository.GetByTicketIdAsync(ticketId);

            if (boardingPass == null)
            {
                return NotFound(new
                {
                    message = "Boarding pass not found."
                });
            }

            return Ok(new
            {
                boardingPass.Id,
                boardingPass.TicketId,

                FlightNumber = ticket.Flight?.FlightNumber ?? string.Empty,

                boardingPass.Gate,
                boardingPass.SequenceNumber,
                boardingPass.IssuedAt,

                QRData = boardingPass.QRCode
            });
        }


    }
}
