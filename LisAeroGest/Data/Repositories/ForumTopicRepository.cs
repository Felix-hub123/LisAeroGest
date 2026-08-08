using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class ForumTopicRepository : GenericRepository<ForumTopic>, IForumTopicRepository
    {
        /// <summary>
        /// Inicializa o ForumTopicRepository com o contexto da base de dados.
        /// </summary>
        /// <param name="context">Contexto da base de dados injetado pelo DI.</param>
        /// <returns>
        /// Instância de <see cref="ForumTopicRepository"/> pronta
        /// a executar queries com eager loading de comentários e utilizadores.
        /// </returns>
        public ForumTopicRepository(DataContext context) : base(context) { }

     
        public async Task<ForumTopic?> GetWithCommentsAsync(int id)
            => await _dbSet
                .Include(t => t.CreatedBy)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

    
        public async Task<IEnumerable<ForumTopic>> GetAllWithDetailsAsync()
            => await _dbSet
                .Include(t => t.CreatedBy)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();


        /// <summary>
        /// Obtém os tópicos mais recentes ordenados por data de criação.
        /// </summary>
        public async Task<IEnumerable<ForumTopic>> GetRecentTopicsAsync(int count)
            => await _dbSet
                .Include(t => t.CreatedBy)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
    }
}
