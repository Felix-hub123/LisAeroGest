using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{

    /// <summary>
    /// ViewModel para o formulário de alteração de password.
    /// Usado por utilizadores autenticados que querem mudar a sua password atual.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Password atual do utilizador — necessária para confirmar a identidade.
        /// </summary>
        /// <param name="OldPassword">Valor introduzido no campo Password Atual.</param>
        /// <returns>
        /// <see cref="string"/> usada para validar a identidade antes de permitir
        /// a alteração via <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/>.
        /// </returns>
        [Required(ErrorMessage = "A password atual é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Atual")]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Nova password escolhida pelo utilizador.
        /// </summary>
        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Password")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Confirmação da nova password — deve ser igual ao campo Nova Password.
        /// </summary>
        [Required(ErrorMessage = "A confirmação de password é obrigatória.")]
        [Compare("NewPassword", ErrorMessage = "As passwords não coincidem.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Password")]
        public string? ConfirmPassword { get; set; }
    }
}
