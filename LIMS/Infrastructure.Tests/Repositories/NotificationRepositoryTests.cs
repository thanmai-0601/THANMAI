using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class NotificationRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddNotification()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("NotificationRepo_Create"));
        var repo = new NotificationRepository(context);
        var note = new Notification { UserId = 1, Message = "Test" };

        // Act
        await repo.CreateAsync(note);

        // Assert
        Assert.Equal(1, await context.Notifications.CountAsync());
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnActiveNotifications()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("NotificationRepo_GetByUser"));
        context.Notifications.Add(new Notification { UserId = 1, Message = "M1", IsRead = false, CreatedAt = DateTime.UtcNow });
        context.Notifications.Add(new Notification { UserId = 1, Message = "M2", IsRead = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repo = new NotificationRepository(context);

        // Act
        var result = await repo.GetByUserIdAsync(1, unreadOnly: true);

        // Assert
        Assert.Single(result);
        Assert.Equal("M1", result[0].Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyNotification()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("NotificationRepo_Update"));
        var note = new Notification { NotificationId = 1, UserId = 1, IsRead = false };
        context.Notifications.Add(note);
        await context.SaveChangesAsync();
        var repo = new NotificationRepository(context);

        // Act
        note.IsRead = true;
        await repo.UpdateAsync(note);

        // Assert
        var updated = await context.Notifications.FindAsync(1);
        Assert.True(updated!.IsRead);
    }
}
