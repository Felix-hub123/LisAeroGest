using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Item genérico para gráficos (label + valor).
    /// </summary>
    public class ChartItemViewModel
    {
        /// <summary>
        /// Etiqueta do ponto no gráfico.
        /// </summary>
        [Display(Name = "Etiqueta")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Valor numérico associado à etiqueta.
        /// </summary>
        [Display(Name = "Quantidade")]
        public int Count { get; set; }
    }
}
