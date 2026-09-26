using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogsController(
            IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }


        // =========================================================
        // HISTÓRICO DE AUDITORIA
        // =========================================================

        public async Task<IActionResult> Index(
            string? search,
            string? action,
            string? category)
        {
            var logs =
                (await _auditLogRepository
                    .GetAllWithDetailsAsync())
                .ToList();


            // Estatísticas são calculadas antes dos filtros.
            var today = DateTime.UtcNow.Date;

            var model = new AuditLogHistoryViewModel
            {
                Search = search,
                Action = action,
                Category = category,

                TotalLogs = logs.Count,

                LogsToday = logs.Count(a =>
                    a.CreatedAt.Date == today),

                CheckIns = logs.Count(a =>
                    a.Action == "CheckIn"),

                GateChanges = logs.Count(a =>
                    a.Action == "GateChanged"),

                Communications = logs.Count(a =>
                    a.Action == "CommunicationSent")
            };


            IEnumerable<AuditLog> filteredLogs = logs;


            // =====================================================
            // PESQUISA GERAL
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                filteredLogs = filteredLogs.Where(a =>
                    ContainsIgnoreCase(
                        a.Description,
                        term) ||

                    ContainsIgnoreCase(
                        a.Action,
                        term) ||

                    ContainsIgnoreCase(
                        a.Category,
                        term) ||

                    ContainsIgnoreCase(
                        a.User?.Email,
                        term) ||

                    ContainsIgnoreCase(
                        a.Flight?.FlightNumber,
                        term) ||

                    (a.TicketId.HasValue &&
                     a.TicketId.Value
                         .ToString()
                         .Contains(term,
                             StringComparison.OrdinalIgnoreCase))
                );
            }


            // =====================================================
            // FILTRO POR AÇÃO
            // =====================================================

            if (!string.IsNullOrWhiteSpace(action))
            {
                filteredLogs = filteredLogs.Where(a =>
                    string.Equals(
                        a.Action,
                        action,
                        StringComparison.OrdinalIgnoreCase));
            }


            // =====================================================
            // FILTRO POR CATEGORIA
            // =====================================================

            if (!string.IsNullOrWhiteSpace(category))
            {
                filteredLogs = filteredLogs.Where(a =>
                    string.Equals(
                        a.Category,
                        category,
                        StringComparison.OrdinalIgnoreCase));
            }


            model.Logs = filteredLogs
                .OrderByDescending(a => a.CreatedAt)
                .Select(MapToItemViewModel)
                .ToList();


            return View(model);
        }


        // =========================================================
        // DETALHES
        // =========================================================

        public async Task<IActionResult> Details(int id)
        {
            var log =
                await _auditLogRepository
                    .GetByIdWithDetailsAsync(id);

            if (log == null)
                return NotFound();


            var model = new AuditLogDetailsViewModel
            {
                Id = log.Id,

                UserName =
                    GetUserName(log),

                UserEmail =
                    log.User?.Email,

                Action =
                    log.Action,

                Category =
                    log.Category,

                Description =
                    log.Description,

                FlightId =
                    log.FlightId,

                FlightNumber =
                    log.Flight?.FlightNumber,

                TicketId =
                    log.TicketId,

                PassengerName =
                    GetPassengerName(log),

                OldValue =
                    log.OldValue,

                NewValue =
                    log.NewValue,

                CreatedAt =
                    log.CreatedAt
            };


            return View(model);
        }


        // =========================================================
        // MAPEAMENTO
        // =========================================================

        private static AuditLogItemViewModel
            MapToItemViewModel(AuditLog log)
        {
            return new AuditLogItemViewModel
            {
                Id = log.Id,

                UserName =
                    GetUserName(log),

                UserEmail =
                    log.User?.Email,

                Action =
                    log.Action,

                Category =
                    log.Category,

                Description =
                    log.Description,

                FlightId =
                    log.FlightId,

                FlightNumber =
                    log.Flight?.FlightNumber,

                TicketId =
                    log.TicketId,

                PassengerName =
                    GetPassengerName(log),

                OldValue =
                    log.OldValue,

                NewValue =
                    log.NewValue,

                CreatedAt =
                    log.CreatedAt
            };
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private static string GetUserName(AuditLog log)
        {
            if (log.User == null)
                return "Sistema";

            var fullName =
                $"{log.User.FirstName} {log.User.LastName}"
                .Trim();

            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;

            if (!string.IsNullOrWhiteSpace(log.User.Email))
                return log.User.Email;

            return "Utilizador";
        }


        private static string? GetPassengerName(
            AuditLog log)
        {
            var passenger =
                log.Ticket?.Passenger;

            if (passenger == null)
                return null;

            var fullName =
                $"{passenger.FirstName} {passenger.LastName}"
                .Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? null
                : fullName;
        }


        private static bool ContainsIgnoreCase(
            string? value,
            string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(
                       search,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}