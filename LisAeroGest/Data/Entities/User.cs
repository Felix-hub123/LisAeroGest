using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class User : IdentityUser
    {
        /// <summary>
        /// Primeiro nome do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apelido do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O apelido não pode exceder 100 caracteres.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Morada completa do utilizador.
        /// </summary>
        [MaxLength(200, ErrorMessage = "A morada não pode passar dos 200 caracteres.")]
        public string? Address { get; set; }

        /// <summary>
        /// Identificador único da imagem de perfil no Azure Blob Storage.
        /// </summary>
        [Display(Name = "Imagem")]
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo da imagem de perfil.
        /// Devolve imagem padrão se não tiver imagem definida.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/users/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/users/{ImageId}.jpg";

        /// <summary>
        /// Nome completo calculado — não é guardado na base de dados.
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Indica se o utilizador já definiu a sua password permanente.
        /// </summary>
        public bool IsPasswordSet { get; set; } = false;
    }
}
