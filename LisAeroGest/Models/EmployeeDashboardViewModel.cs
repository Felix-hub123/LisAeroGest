using System.ComponentModel.DataAnnotations;
using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel do dashboard operacional do funcionário.
    /// Focado nos voos de hoje, check-ins e alertas do turno.
    /// </summary>
    public class EmployeeDashboardViewModel
    {
        /// <summary>
        /// Número de voos programados para hoje.
        /// </summary>
        [Display(Name = "Voos de Hoje")]
        public int TodayFlights { get; set; }

        /// <summary>
        /// Voos com partida na próxima hora.
        /// </summary>
        [Display(Name = "Próxima Hora")]
        public int UpcomingFlights { get; set; }

        /// <summary>
        /// Voos atrasados hoje.
        /// </summary>
        [Display(Name = "Atrasados")]
        public int DelayedToday { get; set; }

        /// <summary>
        /// Voos cancelados hoje.
        /// </summary>
        [Display(Name = "Cancelados")]
        public int CancelledToday { get; set; }

        /// <summary>
        /// Bilhetes pagos ainda sem check-in (voos de hoje).
        /// </summary>
        [Display(Name = "Check-ins Pendentes")]
        public int PendingCheckIns { get; set; }

        /// <summary>
        /// Voos no estado de embarque.
        /// </summary>
        [Display(Name = "A Embarcar")]
        public int BoardingNow { get; set; }

        /// <summary>
        /// Total de voos ativos no sistema.
        /// </summary>
        [Display(Name = "Voos Ativos")]
        public int ActiveFlights { get; set; }

        /// <summary>
        /// Dados para o gráfico de voos por estado.
        /// </summary>
        [Display(Name = "Voos por Estado")]
        public List<ChartItemViewModel> FlightsByStatus { get; set; } = new();

        /// <summary>
        /// Lista detalhada dos voos de hoje.
        /// </summary>
        [Display(Name = "Lista de Voos de Hoje")]
        public List<TodayFlightItemViewModel> TodayFlightList { get; set; } = new();

        /// <summary>
        /// Notificações recentes do funcionário.
        /// </summary>
        [Display(Name = "Notificações")]
        public List<Notification> RecentNotifications { get; set; } = new();

        /// <summary>
        /// Número de notificações não lidas.
        /// </summary>
        [Display(Name = "Não Lidas")]
        public int UnreadCount { get; set; }

        /// <summary>
        /// Nome do funcionário autenticado.
        /// </summary>
        [Display(Name = "Funcionário")]
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora da última atualização.
        /// </summary>
        [Display(Name = "Última Atualização")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}