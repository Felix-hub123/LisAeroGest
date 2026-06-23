using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    /// <summary>
    /// Representa um aeroporto no sistema, com código IATA,
    /// localização e taxa aeroportuária padrão.
    /// </summary>
    public class Airport : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do aeroporto.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome oficial do aeroporto.
        /// </summary>
        [Required(ErrorMessage = "O nome do aeroporto é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string? Name { get; set; }

        /// <summary>
        /// Cidade onde o aeroporto está localizado.
        /// </summary>
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A cidade não pode exceder 100 caracteres.")]
        public string? City { get; set; }

        /// <summary>
        /// País onde o aeroporto está localizado.
        /// </summary>
        [Required(ErrorMessage = "O país é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O país não pode exceder 100 caracteres.")]
        public string? Country { get; set; }

        /// <summary>
        /// Código de três letras da IATA que identifica o aeroporto.
        /// </summary>
        [Required(ErrorMessage = "O código IATA é obrigatório.")]
        [MinLength(3, ErrorMessage = "O código IATA deve ter exatamente 3 caracteres.")]
        [MaxLength(3, ErrorMessage = "O código IATA deve ter exatamente 3 caracteres.")]
        public string? IATACode { get; set; }

      

        /// <summary>
        /// Identificador único da imagem do aeroporto no Azure Blob Storage.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo da imagem do aeroporto.
        /// Devolve imagem padrão se não tiver imagem definida.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/airports/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/airports/{ImageId}";

        /// <summary>
        /// Indica se o aeroporto foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }

        /// <summary>
        /// Indica se o aeroporto já foi usado em voos — não é guardado na base de dados.
        /// </summary>
        [NotMapped]
        public bool IsUsedInFlights { get; set; }

        /// <summary>
        /// Taxa aeroportuária padrão aplicada aos voos neste aeroporto.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultFee { get; set; }


    }
}
