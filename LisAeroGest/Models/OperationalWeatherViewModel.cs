using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel da meteorologia operacional.
    /// </summary>
    public class OperationalWeatherViewModel
    {
        [Display(Name = "Última atualização")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Display(Name = "Localizações")]
        public List<OperationalWeatherItemViewModel> Locations { get; set; } = new();
    }

    /// <summary>
    /// Informação meteorológica de uma localização/aeroporto.
    /// </summary>
    public class OperationalWeatherItemViewModel
    {
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Aeroporto")]
        public string AirportCode { get; set; } = string.Empty;

        [Display(Name = "Temperatura")]
        public double Temperature { get; set; }

        [Display(Name = "Sensação térmica")]
        public double FeelsLike { get; set; }

        [Display(Name = "Humidade")]
        public int Humidity { get; set; }

        [Display(Name = "Condição meteorológica")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ícone")]
        public string Icon { get; set; } = string.Empty;

        [Display(Name = "Velocidade do vento")]
        public double WindSpeedKmh { get; set; }

        [Display(Name = "Visibilidade")]
        public double VisibilityKm { get; set; }

        [Display(Name = "Alerta meteorológico")]
        public bool HasAlert { get; set; }

        [Display(Name = "Aviso operacional")]
        public string? AlertMessage { get; set; }
    }
}