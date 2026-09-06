using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Bilhete do passageiro para listagem no dashboard pessoal.
    /// </summary>
    public class PassengerTicketItemViewModel
    {
        /// <summary>
        /// Identificador do bilhete.
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// Número comercial do voo.
        /// </summary>
        [Display(Name = "Nº do Voo")]
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nome da companhia aérea.
        /// </summary>
        [Display(Name = "Companhia Aérea")]
        public string AirlineName { get; set; } = string.Empty;

        /// <summary>
        /// Código IATA da origem.
        /// </summary>
        [Display(Name = "Origem")]
        public string OriginCode { get; set; } = string.Empty;

        /// <summary>
        /// Código IATA do destino.
        /// </summary>
        [Display(Name = "Destino")]
        public string DestinationCode { get; set; } = string.Empty;

        /// <summary>
        /// Cidade de origem.
        /// </summary>
        [Display(Name = "Cidade de Origem")]
        public string OriginCity { get; set; } = string.Empty;

        /// <summary>
        /// Cidade de destino.
        /// </summary>
        [Display(Name = "Cidade de Destino")]
        public string DestinationCity { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de partida prevista.
        /// </summary>
        [Display(Name = "Partida")]
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Nova hora de partida em caso de atraso.
        /// </summary>
        [Display(Name = "Partida Atrasada")]
        public DateTime? DelayedDepartureTime { get; set; }

        /// <summary>
        /// Número do gate atribuído.
        /// </summary>
        [Display(Name = "Gate")]
        public string? GateNumber { get; set; }

        /// <summary>
        /// Estado do voo.
        /// </summary>
        [Display(Name = "Estado do Voo")]
        public string FlightStatus { get; set; } = string.Empty;

        /// <summary>
        /// Estado do bilhete (Paid, CheckedIn, etc.).
        /// </summary>
        [Display(Name = "Estado do Bilhete")]
        public string TicketStatus { get; set; } = string.Empty;

        /// <summary>
        /// Código do lugar atribuído.
        /// </summary>
        [Display(Name = "Lugar")]
        public string? SeatNumber { get; set; }

        /// <summary>
        /// Indica se o check-in online está disponível.
        /// </summary>
        [Display(Name = "Pode Fazer Check-in")]
        public bool CanCheckIn { get; set; }

        /// <summary>
        /// Indica se já existe cartão de embarque.
        /// </summary>
        [Display(Name = "Tem Cartão de Embarque")]
        public bool HasBoardingPass { get; set; }
    }
}
