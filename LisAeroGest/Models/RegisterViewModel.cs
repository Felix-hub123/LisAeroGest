using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class RegisterViewModel
    {
       

        /// <summary>
        /// Primeiro nome do utilizador a registar.
        /// </summary>
        /// <param name="FirstName">Valor introduzido no campo Nome.</param>
        /// <returns>
        /// <see cref="string"/> validada com máximo de 100 caracteres,
        /// usada para preencher <see cref="Data.Entities.User.FirstName"/>.
        /// </returns>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apelido do utilizador a registar.
        /// </summary>
        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O apelido não pode ter mais de 100 caracteres.")]
        [Display(Name = "Apelido")]
        public string? LastName { get; set; }

        /// <summary>
        /// Endereço de email — usado como username na plataforma.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um email válido.")]
        [Display(Name = "Email")]
        public string? Username { get; set; }

        /// <summary>
        /// Password escolhida pelo utilizador.
        /// </summary>
        [Required(ErrorMessage = "A password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        /// <summary>
        /// Confirmação da password — deve ser igual ao campo Password.
        /// </summary>
        [Required(ErrorMessage = "A confirmação de password é obrigatória.")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Password")]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// Morada do utilizador — opcional no registo.
        /// </summary>
        [MaxLength(200, ErrorMessage = "A morada não pode exceder 200 caracteres.")]
        [Display(Name = "Morada")]
        public string? Address { get; set; }
    }
}
