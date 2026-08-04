using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository

    {

        public NotificationRepository(DataContext context) : base(context) { }


        public async Task<IEnumerable<Notification>> GetByUserAsync(string userId)
              => await _dbSet
                  .Where(n => n.UserId == userId)
                  .OrderByDescending(n => n.CreatedAt)
                  .ToListAsync();

        public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(string userId)
            => await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _dbSet
                .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
