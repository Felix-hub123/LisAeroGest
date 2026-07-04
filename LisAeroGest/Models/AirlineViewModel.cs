using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para criação e edição de companhias aéreas.
    /// </summary>
    public class AirlineViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da companhia aérea é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Companhia Aérea")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O código IATA é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "O código IATA deve ter exatamente 2 caracteres.")]
        [Display(Name = "Código IATA")]
        public string? IATACode { get; set; }

        [Required(ErrorMessage = "O país é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O país não pode exceder 100 caracteres.")]
        [Display(Name = "País")]
        public string? Country { get; set; }

        [Display(Name = "Logótipo")]
        public Guid ImageId { get; set; }

        [Display(Name = "Ficheiro do Logótipo")]
        public IFormFile? ImageFile { get; set; }
    }
}
