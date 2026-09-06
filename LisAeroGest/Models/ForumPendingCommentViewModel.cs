namespace LisAeroGest.Models
{

    /// <summary>
    /// Comentário pendente de moderação para a lista do Admin.
    /// </summary>
    public class ForumPendingCommentViewModel
    {
        public int Id { get; set; }

        public int ForumTopicId { get; set; }

        public string TopicTitle { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public string AuthorInitial { get; set; } = "?";

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
