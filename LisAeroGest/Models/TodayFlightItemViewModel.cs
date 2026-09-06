using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Item da lista de voos de hoje no dashboard do funcionário.
    /// </summary>
    public class TodayFlightItemViewModel
    {
        /// <summary>
        /// Identificador do voo.
        /// </summary>
        public int Id { get; set; }

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
        /// Código IATA do aeroporto de origem.
        /// </summary>
        [Display(Name = "Origem")]
        public string OriginCode { get; set; } = string.Empty;

        /// <summary>
        /// Código IATA do aeroporto de destino.
        /// </summary>
        [Display(Name = "Destino")]
        public string DestinationCode { get; set; } = string.Empty;

        /// <summary>
        /// Hora de partida prevista.
        /// </summary>
        [Display(Name = "Partida")]
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Nova hora de partida em caso de atraso.
        /// </summary>
        [Display(Name = "Partida Atrasada")]
        public DateTime? DelayedDepartureTime { get; set; }

        /// <summary>
        /// Número do gate de embarque.
        /// </summary>
        [Display(Name = "Gate")]
        public string? GateNumber { get; set; }

        /// <summary>
        /// Estado operacional do voo (Scheduled, Boarding, etc.).
        /// </summary>
        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Número de passageiros com bilhete pago ou check-in feito.
        /// </summary>
        [Display(Name = "Passageiros")]
        public int PassengerCount { get; set; }

        /// <summary>
        /// Número de bilhetes pagos ainda sem check-in.
        /// </summary>
        [Display(Name = "Check-ins Pendentes")]
        public int PendingCheckInCount { get; set; }
    }
}