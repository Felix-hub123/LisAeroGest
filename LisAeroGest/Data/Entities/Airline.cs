using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    /// <summary>
    /// Representa uma companhia aérea que opera no aeroporto,
    /// com código IATA, país de origem e logótipo.
    /// </summary>
    public class Airline : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único da companhia aérea.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome oficial da companhia aérea.
        /// </summary>
        [Required(ErrorMessage = "O nome da companhia aérea é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string? Name { get; set; }

        /// <summary>
        /// Código IATA de duas letras que identifica a companhia aérea.
        /// </summary>
        [Required(ErrorMessage = "O código IATA é obrigatório.")]
        [MinLength(2, ErrorMessage = "O código IATA deve ter exatamente 2 caracteres.")]
        [MaxLength(2, ErrorMessage = "O código IATA deve ter exatamente 2 caracteres.")]
        public string? IATACode { get; set; }

        /// <summary>
        /// País de origem da companhia aérea.
        /// </summary>
        [Required(ErrorMessage = "O país é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O país não pode exceder 100 caracteres.")]
        public string? Country { get; set; }

        /// <summary>
        /// Identificador único do logótipo da companhia no Azure Blob Storage.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo do logótipo da companhia aérea.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/airlines/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/airlines/{ImageId}";

        /// <summary>
        /// Lista de voos operados por esta companhia aérea.
        /// </summary>
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();

        /// <summary>
        /// Indica se a companhia aérea foi eliminada logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }

    }
}
