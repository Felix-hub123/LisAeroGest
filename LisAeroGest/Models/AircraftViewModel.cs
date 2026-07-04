using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class AircraftViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(100)]
        [Display(Name = "Marca (Fabricante)")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Modelo")]
        public string? Model { get; set; }

        [Required(ErrorMessage = "O número de lugares em económica é obrigatório.")]
        [Range(1, 500)]
        [Display(Name = "Lugares Económica")]
        public int EconomySeats { get; set; }

        [Required(ErrorMessage = "O número de lugares em executiva é obrigatório.")]
        [Range(0, 100)]
        [Display(Name = "Lugares Executiva")]
        public int BusinessSeats { get; set; }

        [Display(Name = "Disponível para Voos")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Foto da Aeronave")]
        public IFormFile? ImageFile { get; set; }

        public Guid ImageId { get; set; }
    }
}
