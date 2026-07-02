using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para o formulário de redefinição de password.
    /// Usado após o utilizador clicar no link recebido por email.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Token de redefinição gerado pelo Identity — enviado por email ao utilizador.
        /// </summary>
        /// <param name="Token">Token recebido via query string no link do email.</param>
        /// <returns>
        /// <see cref="string"/> passada ao
        /// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}.ResetPasswordAsync"/>
        /// para validar e autorizar a redefinição da password.
        /// </returns>
        [Required]
        public string? Token { get; set; }

        /// <summary>
        /// Email da conta que está a redefinir a password.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Nova password escolhida pelo utilizador.
        /// </summary>
        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Password")]
        public string? Password { get; set; }

        /// <summary>
        /// Confirmação da nova password — deve ser igual ao campo Password.
        /// </summary>
        [Required(ErrorMessage = "A confirmação de password é obrigatória.")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Password")]
        public string? ConfirmPassword { get; set; }
    }
}
