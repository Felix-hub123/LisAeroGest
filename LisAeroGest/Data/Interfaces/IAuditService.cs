using System.Security.Claims;

namespace LisAeroGest.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            ClaimsPrincipal user,
            string action,
            string category,
            string description,
            int? flightId = null,
            int? ticketId = null,
            string? oldValue = null,
            string? newValue = null);
    }
}