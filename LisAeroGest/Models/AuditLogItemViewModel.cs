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


    public class AuditLogHistoryViewModel
    {
        // Filtros
        public string? Search { get; set; }

        public string? Action { get; set; }

        public string? Category { get; set; }


        // Estatísticas
        public int TotalLogs { get; set; }

        public int LogsToday { get; set; }

        public int CheckIns { get; set; }

        public int GateChanges { get; set; }

        public int Communications { get; set; }


        // Resultados
        public List<AuditLogItemViewModel> Logs { get; set; }
            = new();
    }


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