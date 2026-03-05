using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Domain.Enums;
using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IPlanService> _planServiceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _planServiceMock = new Mock<IPlanService>();
        _controller = new AdminController(_authServiceMock.Object, _planServiceMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithUsersList()
    {
        // Arrange
        var users = new List<UserListDto> { new UserListDto { FullName = "Admin User" } };
        _authServiceMock.Setup(s => s.GetAllUsersAsync(null)).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsType<List<UserListDto>>(okResult.Value);
        Assert.Single(returnedUsers);
    }

    [Fact]
    public async Task CreateStaff_ShouldReturnOk_WithStaffResponse()
    {
        // Arrange
        var dto = new CreateStaffDto { FullName = "Agent Smith", Email = "smith@ex.com" };
        var response = new StaffResponseDto { FullName = "Agent Smith" };
        _authServiceMock.Setup(s => s.CreateStaffAsync(dto, UserRole.Agent)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateStaff(UserRole.Agent, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<StaffResponseDto>(okResult.Value);
        Assert.Equal("Agent Smith", returnedResponse.FullName);
    }

    [Fact]
    public async Task ToggleUserStatus_ShouldReturnOk()
    {
        // Arrange
        _authServiceMock.Setup(s => s.ToggleUserStatusAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ToggleUserStatus(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
        _authServiceMock.Verify(s => s.ToggleUserStatusAsync(1), Times.Once);
    }
}
