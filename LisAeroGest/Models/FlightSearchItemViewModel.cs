using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Item de um voo na lista de resultados da pesquisa.
    /// </summary>
    public class FlightSearchItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nº do Voo")]
        public string FlightNumber { get; set; } = string.Empty;

        [Display(Name = "Companhia Aérea")]
        public string AirlineName { get; set; } = string.Empty;

        [Display(Name = "Aeronave")]
        public string? AircraftModel { get; set; }

        [Display(Name = "Origem")]
        public string OriginCode { get; set; } = string.Empty;

        [Display(Name = "Cidade de Origem")]
        public string OriginCity { get; set; } = string.Empty;

        [Display(Name = "Destino")]
        public string DestinationCode { get; set; } = string.Empty;

        [Display(Name = "Cidade de Destino")]
        public string DestinationCity { get; set; } = string.Empty;

        [Display(Name = "Partida")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Chegada")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Preço")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Duração do voo em minutos (para ordenação).
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Duração formatada (ex: 1h 25m).
        /// </summary>
        [Display(Name = "Duração")]
        public string DurationLabel { get; set; } = string.Empty;
    }
}