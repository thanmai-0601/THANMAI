using System.Security.Claims;
using API.Controllers;
using Application.DTOs.Dashboard;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardService> _dashboardServiceMock;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _dashboardServiceMock = new Mock<IDashboardService>();
        _controller = new DashboardController(_dashboardServiceMock.Object);
    }

    private void SetupUser(string role, string id)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role),
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
    public async Task GetDashboardSummary_ShouldReturnAdminDashboard_ForAdmin()
    {
        // Arrange
        SetupUser("Admin", "1");
        _dashboardServiceMock.Setup(s => s.GetAdminDashboardAsync()).ReturnsAsync(new AdminDashboardDto());

        // Act
        var result = await _controller.GetDashboardSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<AdminDashboardDto>(okResult.Value);
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldReturnCustomerDashboard_ForCustomer()
    {
        // Arrange
        SetupUser("Customer", "5");
        _dashboardServiceMock.Setup(s => s.GetCustomerDashboardAsync(5)).ReturnsAsync(new CustomerDashboardDto());

        // Act
        var result = await _controller.GetDashboardSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<CustomerDashboardDto>(okResult.Value);
    }
}
