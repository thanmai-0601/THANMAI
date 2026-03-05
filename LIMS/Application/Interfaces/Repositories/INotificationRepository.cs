using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadOnly);
    Task<Notification?> GetByIdAndUserIdAsync(int notificationId, int userId);
    Task CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
}
