using Application.DTOs.Auth;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WithAuthResponse()
    {
        // Arrange
        var dto = new LoginDto { Email = "test@ex.com", Password = "pw" };
        var response = new AuthResponseDto { Token = "jwt-token" };
        _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync(response);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("jwt-token", returnedResponse.Token);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WithAuthResponse()
    {
        // Arrange
        var dto = new RegisterDto { FullName = "New", Email = "new@ex.com", Password = "pw" };
        var response = new AuthResponseDto { Token = "reg-token" };
        _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(response);

        // Act
        var result = await _controller.Register(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("reg-token", ((AuthResponseDto)okResult.Value!).Token);
    }
}
