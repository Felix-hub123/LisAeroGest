using LisAeroGest.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class Passenger : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do passageiro.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Primeiro nome do passageiro.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Apelido do passageiro.
        /// </summary>
        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O apelido não pode exceder 100 caracteres.")]
        public string? LastName { get; set; }

        /// <summary>
        /// Nome completo calculado — não é guardado na base de dados.
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Identificador único da imagem de perfil no Azure Blob Storage.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo da imagem de perfil.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/users/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/users/{ImageId}";

        /// <summary>
        /// Data em que o passageiro se registou na plataforma.
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tipo de documento de identificação (ex: Cartão de Cidadão, Passaporte).
        /// </summary>
        [Required(ErrorMessage = "O tipo de documento é obrigatório.")]
        [Display(Name = "Tipo de Documento")]
        public string? DocumentType { get; set; }

        /// <summary>
        /// Número do documento de identificação.
        /// </summary>
        [Required(ErrorMessage = "O número do documento é obrigatório.")]
        [Display(Name = "Número do Documento")]
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Data de nascimento do passageiro.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Chave estrangeira para o utilizador associado a este passageiro.
        /// </summary>
        [Required]
        public string? UserId { get; set; }

        /// <summary>
        /// Navegação para o utilizador associado.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Indica se o registo foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}

