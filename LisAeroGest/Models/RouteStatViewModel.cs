using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Estatística de uma rota (origem → destino).
    /// </summary>
    public class RouteStatViewModel
    {
        /// <summary>
        /// Código da rota (ex: LIS → OPO).
        /// </summary>
        [Display(Name = "Rota")]
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// Cidade de origem.
        /// </summary>
        [Display(Name = "Origem")]
        public string Origin { get; set; } = string.Empty;

        /// <summary>
        /// Cidade de destino.
        /// </summary>
        [Display(Name = "Destino")]
        public string Destination { get; set; } = string.Empty;

        /// <summary>
        /// Número de voos nesta rota.
        /// </summary>
        [Display(Name = "Nº de Voos")]
        public int FlightCount { get; set; }
    }
}
