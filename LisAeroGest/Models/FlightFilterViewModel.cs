using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>

    /// ViewModel para a listagem de voos com filtros.

    /// </summary>

    public class FlightFilterViewModel

    {

        /// <summary>

        /// Lista de voos após aplicação dos filtros.

        /// </summary>

        public IEnumerable<Data.Entities.Flight>? Flights { get; set; }


        /// <summary>

        /// SelectList de companhias para o filtro.

        /// </summary>

        public IEnumerable<SelectListItem>? Airlines { get; set; }


        /// <summary>

        /// SelectList de aeroportos para os filtros de origem e destino.

        /// </summary>

        public IEnumerable<SelectListItem>? Airports { get; set; }


        /// <summary>

        /// SelectList de estados para o filtro.

        /// </summary>

        public IEnumerable<SelectListItem>? Statuses { get; set; }


        // ── valores selecionados nos filtros ──────────────────────────────

        public int? FilterAirlineId { get; set; }

        public int? FilterOriginId { get; set; }

        public int? FilterDestinationId { get; set; }

        public string? FilterStatus { get; set; }

    }
}
