using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IForumTopicRepository : IGenericRepository<ForumTopic>
    {
        /// <summary>
        /// Obtém um tópico com os seus comentários e utilizadores carregados.
        /// </summary>
        /// <param name="id">Identificador do tópico.</param>
        /// <returns>
        /// <see cref="ForumTopic"/> com <see cref="ForumTopic.Comments"/>
        /// e <see cref="ForumTopic.CreatedBy"/> carregados,
        /// ou <c>null</c> se não existir.
        /// </returns>
        Task<ForumTopic?> GetWithCommentsAsync(int id);

        /// <summary>
        /// Obtém todos os tópicos com o utilizador criador carregado.
        /// </summary>
        Task<IEnumerable<ForumTopic>> GetAllWithDetailsAsync();
    
    }
}
