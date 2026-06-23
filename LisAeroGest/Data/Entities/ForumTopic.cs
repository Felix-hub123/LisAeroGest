using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class ForumTopic : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do tópico.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Título do tópico de discussão.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Descrição ou conteúdo inicial do tópico.
        /// </summary>
        [Required]
        public string? Content { get; set; }

        /// <summary>
        /// Categoria do tópico (ex: Operations, Maintenance, Security).
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Identificador do utilizador que criou o tópico.
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Navegação para o utilizador que criou o tópico.
        /// </summary>
        public User? CreatedBy { get; set; }

        /// <summary>
        /// Data e hora de criação do tópico.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica se o tópico está fechado para novos comentários.
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Lista de comentários associados a este tópico.
        /// </summary>
        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();

        /// <summary>
        /// Indica se o tópico foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}
