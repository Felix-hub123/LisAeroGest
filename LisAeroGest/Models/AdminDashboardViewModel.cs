using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para o dashboard do administrador.
    /// Contém estatísticas gerais, dados para gráficos e listas resumidas.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// Total de voos registados no sistema.
        /// </summary>
        public int TotalFlights { get; set; }

        /// <summary>
        /// Total de passageiros registados no sistema.
        /// </summary>
        public int TotalPassengers { get; set; }

        /// <summary>
        /// Total de bilhetes vendidos no sistema.
        /// </summary>
        public int TotalTickets { get; set; }

        /// <summary>
        /// Total de companhias aéreas registadas.
        /// </summary>
        public int TotalAirlines { get; set; }

        /// <summary>
        /// Número de voos agendados para hoje.
        /// </summary>
        public int TodayFlights { get; set; }

        /// <summary>
        /// Número de voos atrasados ou cancelados hoje.
        /// </summary>
        public int DisruptedFlights { get; set; }

        /// <summary>
        /// Receita total gerada pelos bilhetes vendidos.
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Dados para o gráfico de voos por estado (labels).
        /// </summary>
        public List<string> FlightStatusLabels { get; set; } = new();

        /// <summary>
        /// Dados para o gráfico de voos por estado (valores).
        /// </summary>
        public List<int> FlightStatusData { get; set; } = new();

        /// <summary>
        /// Dados para o gráfico de voos por companhia aérea (labels).
        /// </summary>
        public List<string> AirlineLabels { get; set; } = new();

        /// <summary>
        /// Dados para o gráfico de voos por companhia aérea (valores).
        /// </summary>
        public List<int> AirlineData { get; set; } = new();

        /// <summary>
        /// Lista dos voos mais recentes para exibição no dashboard.
        /// </summary>
        public List<Flight> RecentFlights { get; set; } = new();

        /// <summary>
        /// Lista dos tópicos mais recentes do fórum interno.
        /// </summary>
        public List<ForumTopic> RecentTopics { get; set; } = new();
    }
}
