using System.Security.Claims;
using API.Controllers;
using Application.DTOs.Notification;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _controller = new NotificationsController(_notificationServiceMock.Object);
    }

    private void SetupUser(string id)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, id)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetMyNotifications_ShouldReturnOk()
    {
        // Arrange
        SetupUser("1");
        _notificationServiceMock.Setup(s => s.GetUserNotificationsAsync(1, false))
            .ReturnsAsync(new List<NotificationDto>());

        // Act
        var result = await _controller.GetMyNotifications();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnOk()
    {
        // Arrange
        SetupUser("1");

        // Act
        var result = await _controller.MarkAsRead(100);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _notificationServiceMock.Verify(s => s.MarkAsReadAsync(100, 1), Times.Once);
    }
}
