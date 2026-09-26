namespace LisAeroGest.Models
{
    /// <summary>
    /// Representa uma comunicação apresentada
    /// no histórico do administrador.
    /// </summary>
    public class FlightCommunicationItemViewModel
    {
        public int Id { get; set; }

        public int FlightId { get; set; }

        public string FlightNumber { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public int RecipientsCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }


    /// <summary>
    /// ViewModel principal da página de histórico.
    /// </summary>
    public class FlightCommunicationHistoryViewModel
    {
        // Filtros
        public string? Search { get; set; }

        public string? Type { get; set; }

        public string? FlightNumber { get; set; }

        // Estatísticas
        public int TotalCommunications { get; set; }

        public int TotalRecipients { get; set; }

        public int CommunicationsToday { get; set; }

        // Lista
        public List<FlightCommunicationItemViewModel> Communications { get; set; }
            = new();
    }


    /// <summary>
    /// ViewModel utilizado para consultar
    /// uma comunicação individual.
    /// </summary>
    public class FlightCommunicationDetailsViewModel
    {
        public int Id { get; set; }

        public int FlightId { get; set; }

        public string FlightNumber { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string? EmployeeEmail { get; set; }

        public int RecipientsCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}