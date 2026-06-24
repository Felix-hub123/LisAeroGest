using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class LoginViewModel
    {
        /// <summary>
        /// Email do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress]
        public string? Username { get; set; }

        /// <summary>
        /// Password do utilizador.
        /// </summary>
        [Required(ErrorMessage = "A password é obrigatória.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        /// <summary>
        /// Indica se a sessão deve ser mantida após fechar o browser.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
