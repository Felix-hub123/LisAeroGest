using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class ForumComment : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do comentário.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Conteúdo do comentário.
        /// </summary>
        [Required]
        public string? Content { get; set; }

        /// <summary>
        /// Chave estrangeira para o tópico ao qual o comentário pertence.
        /// </summary>
        public int ForumTopicId { get; set; }

        /// <summary>
        /// Navegação para o tópico associado.
        /// </summary>
        public ForumTopic? ForumTopic { get; set; }

        /// <summary>
        /// Identificador do utilizador que escreveu o comentário.
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Navegação para o utilizador que escreveu o comentário.
        /// </summary>
        public User? CreatedBy { get; set; }

        /// <summary>
        /// Data e hora em que o comentário foi criado.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica se o comentário foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}