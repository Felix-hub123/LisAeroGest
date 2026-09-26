using LisAeroGest.Data;
using LisAeroGest.Data.Entities;
using System.Security.Claims;

namespace LisAeroGest.Services
{
    public class AuditService : IAuditService
    {
        private readonly DataContext _context;

        public AuditService(DataContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            ClaimsPrincipal user,
            string action,
            string category,
            string description,
            int? flightId = null,
            int? ticketId = null,
            string? oldValue = null,
            string? newValue = null)
        {
            var userId =
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            var auditLog = new AuditLog
            {
                UserId = userId,

                Action = action,

                Category = category,

                Description = description,

                FlightId = flightId,

                TicketId = ticketId,

                OldValue = oldValue,

                NewValue = newValue,

                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }
    }
}