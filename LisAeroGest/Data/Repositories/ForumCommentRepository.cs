using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    /// <summary>
    /// Repositório especializado para a gestão de comentários do fórum.
    /// </summary>
    public class ForumCommentRepository : GenericRepository<ForumComment>, IForumCommentRepository
    {
        public ForumCommentRepository(DataContext context) : base(context)
        {
        }

        /// <summary>
        /// Retorna comentários pendentes de moderação, ordenados do mais antigo para o mais recente,
        /// incluindo os dados do tópico e do utilizador criador.
        /// </summary>
        public async Task<IEnumerable<ForumComment>> GetPendingCommentsAsync()
        {
            return await _dbSet
                .Include(c => c.ForumTopic)
                .Include(c => c.CreatedBy)
                .Where(c => !c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna apenas os comentários aprovados de um determinado tópico com o perfil do autor.
        /// </summary>
        public async Task<IEnumerable<ForumComment>> GetApprovedCommentsByTopicIdAsync(int topicId)
        {
            return await _dbSet
                .Include(c => c.CreatedBy)
                .Where(c => c.ForumTopicId == topicId && c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ForumComment>> GetVisibleCommentsForUserAsync(int topicId, string userEmail, bool isAdmin)
        {
            var query = _dbSet
                .Include(c => c.CreatedBy)
                .Where(c => c.ForumTopicId == topicId);

            if (!isAdmin)
            {
                // Se NÃO for Admin, só traz aprovados OU criados pelo próprio utilizador
                query = query.Where(c => c.IsApproved || (c.CreatedBy != null && c.CreatedBy.Email == userEmail));
            }

            return await query.OrderBy(c => c.CreatedAt).ToListAsync();
        }
    }
}
