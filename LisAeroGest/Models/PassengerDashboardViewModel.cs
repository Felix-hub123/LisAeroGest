using System.ComponentModel.DataAnnotations;
using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel do dashboard pessoal do passageiro.
    /// Mostra apenas as viagens e ações do próprio utilizador.
    /// </summary>
    public class PassengerDashboardViewModel
    {
        // ═══════════════════════════════════════════════════
        // PERFIL
        // ═══════════════════════════════════════════════════

        public Passenger Passenger { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        // ═══════════════════════════════════════════════════
        // VOOS
        // ═══════════════════════════════════════════════════

        public List<PassengerTicketItemViewModel> UpcomingTickets { get; set; } = new();

        public List<PassengerTicketItemViewModel> PastTickets { get; set; } = new();

        /// <summary>
        /// Reservas pendentes (Status = "Reserved" e ainda válidas).
        /// </summary>
        public List<PassengerTicketItemViewModel> PendingReservations { get; set; } = new();

        public PassengerTicketItemViewModel? NextFlight { get; set; }

        // ═══════════════════════════════════════════════════
        // KPIs
        // ═══════════════════════════════════════════════════

        public int UpcomingCount { get; set; }

        public int PastCount { get; set; }

        public int PendingCount { get; set; }

        public decimal TotalSpent { get; set; }

        // ═══════════════════════════════════════════════════
        // NOTIFICAÇÕES
        // ═══════════════════════════════════════════════════

        public List<Notification> RecentNotifications { get; set; } = new();

        public int UnreadNotificationsCount { get; set; }

        // ═══════════════════════════════════════════════════
        // LIS AERO POINTS (fidelização)
        // ═══════════════════════════════════════════════════

        public int LoyaltyPoints { get; set; }

        public string LoyaltyTier { get; set; } = "Bronze";

        public int PointsToNextTier { get; set; }

        public int LoyaltyProgressPercent { get; set; }  // 0-100
    }
}