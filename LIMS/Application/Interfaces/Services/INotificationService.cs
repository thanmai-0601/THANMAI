using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Notification;

namespace Application.Interfaces.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task CreateNotificationAsync(int userId, string message);
}
