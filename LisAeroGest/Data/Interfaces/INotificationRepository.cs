using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification> 
    {
        Task<IEnumerable<Notification>> GetByUserAsync(string userId);

        Task<IEnumerable<Notification>> GetUnreadByUserAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);
    }
}
