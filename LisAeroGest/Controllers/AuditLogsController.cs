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
        private const int PageSize = 10;

        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogsController(
            IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }


        // =========================================================
        // HISTÓRICO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? auditAction,
            string? category,
            int page = 1)
        {
            var logs =
                (await _auditLogRepository
                    .GetAllWithDetailsAsync())
                .ToList();


            // =====================================================
            // NÃO MOSTRAR CREATE
            // =====================================================

            var filteredLogs = logs
                .Where(a =>
                    !string.Equals(
                        a.Action,
                        "Create",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();


            // =====================================================
            // NORMALIZAR FILTROS
            // =====================================================

            search = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            auditAction = string.IsNullOrWhiteSpace(auditAction)
                ? null
                : auditAction.Trim();

            category = string.IsNullOrWhiteSpace(category)
                ? null
                : category.Trim();


            // =====================================================
            // ESTATÍSTICAS
            // =====================================================

            var today = DateTime.UtcNow.Date;


            var model = new AuditLogHistoryViewModel
            {
                Search = search,

                Action = auditAction,

                Category = category,

                TotalLogs = filteredLogs.Count,

                LogsToday = filteredLogs.Count(a =>
                    a.CreatedAt.Date == today),

                CheckIns = filteredLogs.Count(a =>
                    a.Action == "CheckIn"),

                GateChanges = filteredLogs.Count(a =>
                    a.Action == "GateChanged"),

                Communications = filteredLogs.Count(a =>
                    a.Action == "CommunicationSent")
            };


            // =====================================================
            // PESQUISA
            // =====================================================

            if (search != null)
            {
                filteredLogs = filteredLogs
                    .Where(a =>

                        ContainsIgnoreCase(
                            a.Description,
                            search)

                        ||

                        ContainsIgnoreCase(
                            a.Action,
                            search)

                        ||

                        ContainsIgnoreCase(
                            a.Category,
                            search)

                        ||

                        ContainsIgnoreCase(
                            a.User?.Email,
                            search)

                        ||

                        ContainsIgnoreCase(
                            GetUserName(a),
                            search)

                        ||

                        ContainsIgnoreCase(
                            a.Flight?.FlightNumber,
                            search)

                        ||

                        (
                            a.TicketId.HasValue &&
                            a.TicketId.Value
                                .ToString()
                                .Contains(
                                    search,
                                    StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    .ToList();
            }


            // =====================================================
            // FILTRO POR AÇÃO
            // =====================================================

            if (auditAction != null)
            {
                filteredLogs = filteredLogs
                    .Where(a =>
                        string.Equals(
                            a.Action,
                            auditAction,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }


            // =====================================================
            // FILTRO POR CATEGORIA
            // =====================================================

            if (category != null)
            {
                filteredLogs = filteredLogs
                    .Where(a =>
                        string.Equals(
                            a.Category,
                            category,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }


            // =====================================================
            // PAGINAÇÃO
            // =====================================================

            var filteredCount =
                filteredLogs.Count;


            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    filteredCount /
                    (double)PageSize));


            page = Math.Max(
                1,
                Math.Min(
                    page,
                    totalPages));


            model.FilteredCount =
                filteredCount;

            model.Page =
                page;

            model.PageSize =
                PageSize;

            model.TotalPages =
                totalPages;


            model.Logs = filteredLogs
                .OrderByDescending(a =>
                    a.CreatedAt)
                .Skip(
                    (page - 1) *
                    PageSize)
                .Take(PageSize)
                .Select(
                    MapToItemViewModel)
                .ToList();


            return View(model);
        }


        // =========================================================
        // DETALHES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var log =
                await _auditLogRepository
                    .GetByIdWithDetailsAsync(id);


            if (log == null)
            {
                return NotFound();
            }


            var model =
                new AuditLogDetailsViewModel
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

        private static string GetUserName(
            AuditLog log)
        {
            if (log.User == null)
            {
                return "Sistema";
            }


            var fullName =
                $"{log.User.FirstName} {log.User.LastName}"
                    .Trim();


            if (!string.IsNullOrWhiteSpace(fullName))
            {
                return fullName;
            }


            if (!string.IsNullOrWhiteSpace(
                log.User.Email))
            {
                return log.User.Email;
            }


            return "Utilizador";
        }


        private static string? GetPassengerName(
            AuditLog log)
        {
            var passenger =
                log.Ticket?.Passenger;


            if (passenger == null)
            {
                return null;
            }


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
            return
                !string.IsNullOrWhiteSpace(value)
                &&
                value.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}