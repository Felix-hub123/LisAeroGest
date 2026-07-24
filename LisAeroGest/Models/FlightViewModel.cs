using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>

    /// ViewModel para criação e edição de voos.

    /// </summary>

    public class FlightViewModel

    {

        /// <summary>

        /// Identificador do voo (usado na edição).

        /// </summary>

        public int Id { get; set; }


        /// <summary>

        /// Código comercial do voo (ex: TP1402).

        /// </summary>

        [Required(ErrorMessage = "O número do voo é obrigatório.")]

        [MaxLength(10, ErrorMessage = "O número do voo não pode exceder 10 caracteres.")]

        [Display(Name = "Número do Voo")]

        public string? FlightNumber { get; set; }


        /// <summary>

        /// Identificador da companhia aérea.

        /// </summary>

        [Required(ErrorMessage = "A companhia aérea é obrigatória.")]

        [Display(Name = "Companhia Aérea")]

        public int AirlineId { get; set; }


        /// <summary>

        /// Lista de companhias aéreas para popular o dropdown.

        /// </summary>

        public IEnumerable<SelectListItem>? Airlines { get; set; }


        /// <summary>

        /// Identificador do aeroporto de origem.

        /// </summary>

        [Required(ErrorMessage = "O aeroporto de origem é obrigatório.")]

        [Display(Name = "Origem")]

        public int OriginAirportId { get; set; }


        /// <summary>

        /// Lista de aeroportos para popular os dropdowns de origem e destino.

        /// </summary>

        public IEnumerable<SelectListItem>? Airports { get; set; }


        /// <summary>

        /// Identificador do aeroporto de destino.

        /// </summary>

        [Required(ErrorMessage = "O aeroporto de destino é obrigatório.")]

        [Display(Name = "Destino")]

        public int DestinationAirportId { get; set; }


        /// <summary>

        /// Identificador da aeronave.

        /// </summary>

        [Required(ErrorMessage = "A aeronave é obrigatória.")]

        [Display(Name = "Aeronave")]

        public int AircraftId { get; set; }


        /// <summary>

        /// Lista de aeronaves disponíveis para popular o dropdown.

        /// </summary>

        public IEnumerable<SelectListItem>? Aircrafts { get; set; }


        /// <summary>

        /// Identificador do gate de embarque (opcional).

        /// </summary>

        [Display(Name = "Gate (Opcional)")]

        public int? GateId { get; set; }


        /// <summary>

        /// Lista de gates disponíveis para popular o dropdown.

        /// </summary>

        public IEnumerable<SelectListItem>? Gates { get; set; }


        /// <summary>

        /// Data e hora de partida.

        /// </summary>

        [Required(ErrorMessage = "A data e hora de partida são obrigatórias.")]

        [Display(Name = "Partida")]

        [DataType(DataType.DateTime)]

        public DateTime DepartureTime { get; set; } = DateTime.Now;


        /// <summary>

        /// Data e hora de chegada.

        /// </summary>

        [Required(ErrorMessage = "A data e hora de chegada são obrigatórias.")]

        [Display(Name = "Chegada")]

        [DataType(DataType.DateTime)]

        public DateTime ArrivalTime { get; set; } = DateTime.Now.AddHours(2);


        /// <summary>

        /// Preço base do bilhete para este voo.

        /// </summary>

        [Required(ErrorMessage = "O preço base é obrigatório.")]

        [Range(0, 10000)]

        [Display(Name = "Preço Base (€)")]

        public decimal BasePrice { get; set; }


        /// <summary>

        /// Estado operacional do voo (Previsto, CheckIn, Boarding, Departed, Delayed, Cancelled).

        /// </summary>

        [Display(Name = "Estado")]

        public string Status { get; set; } = "Scheduled";


        /// <summary>

        /// Lista de estados possíveis para popular o dropdown.

        /// </summary>

        public IEnumerable<SelectListItem>? Statuses { get; set; }

    }
}
