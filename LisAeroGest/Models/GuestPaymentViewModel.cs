using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Dados do ecrã de pagamento do visitante.
    /// </summary>
    public class GuestPaymentViewModel
    {
        public int TicketId { get; set; }

        [Display(Name = "Nº do Voo")]
        public string FlightNumber { get; set; } = string.Empty;

        [Display(Name = "Origem")]
        public string OriginCode { get; set; } = string.Empty;

        [Display(Name = "Destino")]
        public string DestinationCode { get; set; } = string.Empty;

        [Display(Name = "Lugar")]
        public string SeatCode { get; set; } = string.Empty;

        [Display(Name = "Passageiro")]
        public string PassengerName { get; set; } = string.Empty;

        [Display(Name = "Bagagem extra")]
        public bool ExtraLuggage { get; set; }

        [Display(Name = "Refeição")]
        public bool MealIncluded { get; set; }

        [Display(Name = "Total")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Expira em")]
        public DateTime? ExpiresAt { get; set; }
    }
}