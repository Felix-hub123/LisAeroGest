using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel do dashboard administrativo.
    /// Contém KPIs, dados de gráficos, alertas e informação de gestão.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// Total de voos registados (não eliminados).
        /// </summary>
        [Display(Name = "Total de Voos")]
        public int TotalFlights { get; set; }

        /// <summary>
        /// Voos em operação (Previsto, Check-in ou A Embarcar).
        /// </summary>
        [Display(Name = "Voos em Operação")]
        public int ActiveFlights { get; set; }

        /// <summary>
        /// Número de voos atrasados.
        /// </summary>
        [Display(Name = "Voos Atrasados")]
        public int DelayedFlights { get; set; }

        /// <summary>
        /// Número de voos cancelados.
        /// </summary>
        [Display(Name = "Voos Cancelados")]
        public int CancelledFlights { get; set; }

        /// <summary>
        /// Total de bilhetes vendidos.
        /// </summary>
        [Display(Name = "Bilhetes Vendidos")]
        public int TotalTickets { get; set; }

        /// <summary>
        /// Receita total dos bilhetes pagos e com check-in.
        /// </summary>
        [Display(Name = "Receita Total")]
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Taxa média de ocupação dos voos (percentagem).
        /// </summary>
        [Display(Name = "Taxa de Ocupação")]
        public double OccupancyRate { get; set; }

        /// <summary>
        /// Número de lugares ocupados.
        /// </summary>
        [Display(Name = "Lugares Ocupados")]
        public int OccupiedSeats { get; set; }

        /// <summary>
        /// Número total de lugares disponíveis.
        /// </summary>
        [Display(Name = "Total de Lugares")]
        public int TotalSeats { get; set; }

        /// <summary>
        /// Dados para o gráfico de voos por companhia aérea.
        /// </summary>
        [Display(Name = "Voos por Companhia")]
        public List<ChartItemViewModel> FlightsByAirline { get; set; } = new();

        /// <summary>
        /// Dados para o gráfico de voos por estado.
        /// </summary>
        [Display(Name = "Voos por Estado")]
        public List<ChartItemViewModel> FlightsByStatus { get; set; } = new();

        /// <summary>
        /// Receita mensal dos últimos 12 meses.
        /// </summary>
        [Display(Name = "Receita por Mês")]
        public List<RevenueMonthViewModel> RevenueByMonth { get; set; } = new();

        /// <summary>
        /// Top 5 rotas mais populares.
        /// </summary>
        [Display(Name = "Rotas Mais Populares")]
        public List<RouteStatViewModel> TopRoutes { get; set; } = new();

        /// <summary>
        /// Receita e bilhetes por companhia aérea.
        /// </summary>
        [Display(Name = "Receita por Companhia")]
        public List<AirlineRevenueViewModel> RevenueByAirline { get; set; } = new();

        /// <summary>
        /// Alertas administrativos relevantes.
        /// </summary>
        [Display(Name = "Alertas")]
        public List<DashboardAlertViewModel> Alerts { get; set; } = new();

        /// <summary>
        /// Nome do administrador autenticado.
        /// </summary>
        [Display(Name = "Administrador")]
        public string AdminName { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora da última atualização dos dados.
        /// </summary>
        [Display(Name = "Última Atualização")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}