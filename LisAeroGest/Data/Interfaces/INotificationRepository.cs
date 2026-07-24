using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserAsync(string userId);

        Task<IEnumerable<Notification>> GetUnreadByUserAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task AddAsync(Notification notification);

        Task AddRangeAsync(IEnumerable<Notification> notifications);

        Task UpdateAsync(Notification notification);  

        Task DeleteAsync(Notification notification);

        Task SaveAsync();
    }
}
