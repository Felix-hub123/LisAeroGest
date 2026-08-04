using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class AircraftViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione o fabricante da aeronave.")]
        [Display(Name = "Fabricante")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "O modelo é de preenchimento obrigatório.")]
        [Display(Name = "Modelo")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Lista de modelos filtrada por fabricante para o dropdown encadeado.
        /// </summary>
        public IEnumerable<SelectListItem>? Models { get; set; }

        [Required(ErrorMessage = "O número de lugares em classe económica é obrigatório.")]
        [Range(1, 500, ErrorMessage = "Os lugares em classe económica devem estar entre 1 e 500.")]
        [Display(Name = "Lugares Económica")]
        public int EconomySeats { get; set; }

        [Required(ErrorMessage = "O número de lugares em classe executiva é obrigatório.")]
        [Range(0, 100, ErrorMessage = "Os lugares em classe executiva devem estar entre 0 e 100.")]
        [Display(Name = "Lugares Executiva")]
        public int BusinessSeats { get; set; }

        [Display(Name = "Disponível")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Imagem")]
        public IFormFile? ImageFile { get; set; }

        public Guid ImageId { get; set; }

        // LISTA DE SELEÇÃO DE MARCAS (Gerada pelo Backend)
        public IEnumerable<SelectListItem>? Brands { get; set; }
    }
}
