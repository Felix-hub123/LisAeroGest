using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para o formulário de recuperação de password.
    /// O utilizador introduz o email e recebe um link para redefinir a password.
    /// </summary>
    public class RecoverPasswordViewModel
    {
        /// <summary>
        /// Email da conta para a qual se pretende recuperar a password.
        /// </summary>
        /// <param name="Email">Endereço de email introduzido pelo utilizador.</param>
        /// <returns>
        /// <see cref="string"/> usada para localizar o utilizador via
        /// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}.FindByEmailAsync"/>.
        /// </returns>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um email válido.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}
