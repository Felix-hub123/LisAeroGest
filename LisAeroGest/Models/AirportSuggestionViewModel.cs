using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Sugestão de aeroporto para autocomplete.
    /// </summary>
    public class AirportSuggestionViewModel
    {
        [Display(Name = "Código IATA")]
        public string Iata { get; set; } = string.Empty;

        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "País")]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "Aeroporto")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Texto mostrado no campo (ex: Porto (OPO)).
        /// </summary>
        [Display(Name = "Etiqueta")]
        public string Label { get; set; } = string.Empty;
    }
}