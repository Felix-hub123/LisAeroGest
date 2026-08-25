using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class GuestCheckoutViewModel
    {

        public int FlightId { get; set; }
        public int SeatId { get; set; }
        public bool ExtraLuggage { get; set; }
        public bool MealIncluded { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O documento de identificação é obrigatório.")]
        [Display(Name = "NIF / Passaporte / Cartão Cidadão")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Display(Name = "Quero criar uma conta após a compra")]
        public bool WantToCreateAccount { get; set; }
    }
}
