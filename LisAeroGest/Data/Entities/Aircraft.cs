using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class Aircraft : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único da aeronave.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fabricante da aeronave (ex: Boeing, Airbus).
        /// </summary>
        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A marca não pode exceder 100 caracteres.")]
        public string? Brand { get; set; }

        /// <summary>
        /// Modelo comercial da aeronave (ex: 737-800, A320neo).
        /// </summary>
        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O modelo não pode exceder 100 caracteres.")]
        public string? Model { get; set; }

        /// <summary>
        /// Número de lugares disponíveis na classe económica.
        /// </summary>
        [Required(ErrorMessage = "O número de lugares em classe económica é obrigatório.")]
        [Range(1, 500, ErrorMessage = "Os lugares em classe económica devem estar entre 1 e 500.")]
        public int EconomySeats { get; set; }

        /// <summary>
        /// Número de lugares disponíveis na classe executiva.
        /// </summary>
        [Required(ErrorMessage = "O número de lugares em classe executiva é obrigatório.")]
        [Range(0, 100, ErrorMessage = "Os lugares em classe executiva devem estar entre 0 e 100.")]
        public int BusinessSeats { get; set; }

        /// <summary>
        /// Capacidade total da aeronave — soma das duas classes.
        /// Não é guardado na base de dados.
        /// </summary>
        [NotMapped]
        public int TotalCapacity => EconomySeats + BusinessSeats;

        /// <summary>
        /// Identificador único da imagem da aeronave no Azure Blob Storage.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo da imagem da aeronave.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/aircraft/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/aircraft/{ImageId}";

        /// <summary>
        /// Indica se a aeronave está disponível para operar voos.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Lista de assentos físicos desta aeronave.
        /// </summary>
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        /// <summary>
        /// Indica se a aeronave foi eliminada logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}