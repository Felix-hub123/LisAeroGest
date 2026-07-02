using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para edição do perfil do utilizador autenticado.
    /// Permite alterar dados pessoais e foto de perfil.
    /// </summary>
    public class UserProfileViewModel
    {
        /// <summary>
        /// Primeiro nome do utilizador.
        /// </summary>
        /// <param name="FirstName">Valor introduzido no campo Nome.</param>
        /// <returns>
        /// <see cref="string"/> validada usada para atualizar
        /// <see cref="Data.Entities.User.FirstName"/> via UserManager.
        /// </returns>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apelido do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [MaxLength(100)]
        [Display(Name = "Apelido")]
        public string? LastName { get; set; }

        /// <summary>
        /// Morada do utilizador.
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Morada")]
        public string? Address { get; set; }

        /// <summary>
        /// Ficheiro de imagem de perfil enviado pelo utilizador — opcional.
        /// </summary>
        [Display(Name = "Foto de Perfil")]
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// URL atual da imagem de perfil — usado para mostrar a foto existente.
        /// </summary>
        public string? ImageFullPath { get; set; }
    }
}
