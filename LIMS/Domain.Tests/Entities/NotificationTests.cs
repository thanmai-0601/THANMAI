using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class NotificationTests
{
    [Fact]
    public void Notification_Initialization_ShouldBeUnreadByDefault()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Notification_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var notification = new Notification();
        var message = "Your policy is approved.";

        // Act
        notification.Message = message;
        notification.IsRead = true;

        // Assert
        Assert.Equal(message, notification.Message);
        Assert.True(notification.IsRead);
    }
}
