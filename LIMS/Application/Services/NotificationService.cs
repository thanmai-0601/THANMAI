using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Notification;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;

    public NotificationService(INotificationRepository notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var notifications = await _notificationRepo.GetByUserIdAsync(userId, unreadOnly);

        return notifications
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepo.GetByIdAndUserIdAsync(notificationId, userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);
        }
    }

    public async Task CreateNotificationAsync(int userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false
        };

        await _notificationRepo.CreateAsync(notification);
    }
}
