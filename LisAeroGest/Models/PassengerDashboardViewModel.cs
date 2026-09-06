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
        /// <summary>
        /// Dados do perfil do passageiro.
        /// </summary>
        [Display(Name = "Passageiro")]
        public Passenger Passenger { get; set; } = null!;

        /// <summary>
        /// Nome completo do passageiro.
        /// </summary>
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Bilhetes de viagens futuras.
        /// </summary>
        [Display(Name = "Próximos Voos")]
        public List<PassengerTicketItemViewModel> UpcomingTickets { get; set; } = new();

        /// <summary>
        /// Bilhetes de viagens já realizadas ou canceladas.
        /// </summary>
        [Display(Name = "Histórico de Voos")]
        public List<PassengerTicketItemViewModel> PastTickets { get; set; } = new();

        /// <summary>
        /// Próxima viagem em destaque (primeiro da lista de upcoming).
        /// </summary>
        [Display(Name = "Próxima Viagem")]
        public PassengerTicketItemViewModel? NextFlight { get; set; }

        /// <summary>
        /// Quantidade de voos futuros.
        /// </summary>
        [Display(Name = "Nº de Próximos Voos")]
        public int UpcomingCount { get; set; }

        /// <summary>
        /// Quantidade de voos no histórico.
        /// </summary>
        [Display(Name = "Nº de Voos Realizados")]
        public int PastCount { get; set; }
    }
}