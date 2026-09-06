using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Alerta apresentado no dashboard administrativo.
    /// </summary>
    public class DashboardAlertViewModel
    {
        /// <summary>
        /// Título do alerta.
        /// </summary>
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem descritiva do alerta.
        /// </summary>
        [Display(Name = "Mensagem")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Severidade: info, warning ou danger.
        /// </summary>
        [Display(Name = "Severidade")]
        public string Severity { get; set; } = "info";

        /// <summary>
        /// Classe do ícone Bootstrap Icons.
        /// </summary>
        [Display(Name = "Ícone")]
        public string Icon { get; set; } = "bi-info-circle";

        /// <summary>
        /// Ligação opcional para mais detalhes.
        /// </summary>
        [Display(Name = "Ligação")]
        public string? Link { get; set; }
    }
}
