using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IForumTopicRepository : IGenericRepository<ForumTopic>
    {
        /// <summary>
        /// Obtém um tópico com os seus comentários e utilizadores carregados.
        /// </summary>
        Task<ForumTopic?> GetWithCommentsAsync(int id);

        /// <summary>
        /// Obtém todos os tópicos com o utilizador criador carregado.
        /// </summary>
        Task<IEnumerable<ForumTopic>> GetAllWithDetailsAsync();

        /// <summary>
        /// Obtém os tópicos mais recentes para o dashboard.
        /// </summary>
        /// <param name="count">Número de tópicos a devolver.</param>
        Task<IEnumerable<ForumTopic>> GetRecentTopicsAsync(int count);

    }
}
