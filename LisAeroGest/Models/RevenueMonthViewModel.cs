using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Receita agregada por mês (últimos 12 meses).
    /// </summary>
    public class RevenueMonthViewModel
    {
        /// <summary>
        /// Identificador do mês no formato AAAA-MM.
        /// </summary>
        [Display(Name = "Mês")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Total de receita no mês.
        /// </summary>
        [Display(Name = "Receita")]
        public decimal Total { get; set; }
    }
}
