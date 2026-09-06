using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Receita e bilhetes vendidos por companhia aérea.
    /// </summary>
    public class AirlineRevenueViewModel
    {
        /// <summary>
        /// Nome da companhia aérea.
        /// </summary>
        [Display(Name = "Companhia Aérea")]
        public string Airline { get; set; } = string.Empty;

        /// <summary>
        /// Número de bilhetes vendidos.
        /// </summary>
        [Display(Name = "Bilhetes")]
        public int Tickets { get; set; }

        /// <summary>
        /// Receita total da companhia.
        /// </summary>
        [Display(Name = "Receita")]
        public decimal Revenue { get; set; }
    }
}
