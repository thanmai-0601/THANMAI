using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repoMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repoMock = new Mock<INotificationRepository>();
        _service = new NotificationService(_repoMock.Object);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnDtos()
    {
        // Arrange
        var notes = new List<Notification>
        {
            new Notification { NotificationId = 1, Message = "M1", CreatedAt = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.GetByUserIdAsync(1, false)).ReturnsAsync(notes);

        // Act
        var result = await _service.GetUserNotificationsAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("M1", result[0].Message);
    }

    [Fact]
    public async Task CreateNotificationAsync_ShouldCallRepo()
    {
        // Act
        await _service.CreateNotificationAsync(1, "Welcome");

        // Assert
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Once);
    }
}
