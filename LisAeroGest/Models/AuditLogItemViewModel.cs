namespace LisAeroGest.Models
{
    public class AuditLogItemViewModel
    {
        public int Id { get; set; }

        public string UserName { get; set; } = "Sistema";

        public string? UserEmail { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? FlightId { get; set; }

        public string? FlightNumber { get; set; }

        public int? TicketId { get; set; }

        public string? PassengerName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedAt { get; set; }
    }


    // =============================================================
    // HISTÓRICO
    // =============================================================

    public class AuditLogHistoryViewModel
    {
        // =========================================================
        // FILTROS
        // =========================================================

        public string? Search { get; set; }

        public string? Action { get; set; }

        public string? Category { get; set; }


        // =========================================================
        // ESTATÍSTICAS
        // =========================================================

        public int TotalLogs { get; set; }

        public int LogsToday { get; set; }

        public int CheckIns { get; set; }

        public int GateChanges { get; set; }

        public int Communications { get; set; }


        // =========================================================
        // PAGINAÇÃO
        // =========================================================

        public int FilteredCount { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; } = 1;


        public bool HasPreviousPage =>
            Page > 1;


        public bool HasNextPage =>
            Page < TotalPages;


        public int FirstItem =>
            FilteredCount == 0
                ? 0
                : ((Page - 1) * PageSize) + 1;


        public int LastItem =>
            Math.Min(
                Page * PageSize,
                FilteredCount);


        // =========================================================
        // RESULTADOS
        // =========================================================

        public List<AuditLogItemViewModel> Logs { get; set; }
            = new();
    }


    // =============================================================
    // DETALHES
    // =============================================================

    public class AuditLogDetailsViewModel
    {
        public int Id { get; set; }

        public string UserName { get; set; } = "Sistema";

        public string? UserEmail { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? FlightId { get; set; }

        public string? FlightNumber { get; set; }

        public int? TicketId { get; set; }

        public string? PassengerName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}