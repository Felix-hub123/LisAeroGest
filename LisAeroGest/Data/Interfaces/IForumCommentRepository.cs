using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    /// <summary>
    /// Interface para o repositório de comentários do fórum.
    /// Herda as operações CRUD padrão do IGenericRepository.
    /// </summary>
    public interface IForumCommentRepository : IGenericRepository<ForumComment>
    {
        /// <summary>
        /// Obtém todos os comentários pendentes de aprovação para moderação.
        /// </summary>
        Task<IEnumerable<ForumComment>> GetPendingCommentsAsync();

        /// <summary>
        /// Obtém todos os comentários de um determinado tópico incluindo o utilizador autor.
        /// </summary>
        Task<IEnumerable<ForumComment>> GetApprovedCommentsByTopicIdAsync(int topicId);

        /// <summary>
        /// Obtém comentários visíveis para o utilizador atual (Aprovados + Pendentes próprios + Todos se for Admin).
        /// </summary>
        Task<IEnumerable<ForumComment>> GetVisibleCommentsForUserAsync(int topicId, string userEmail, bool isAdmin);
    }
}
