using System.Net;
using API.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly GlobalExceptionMiddleware _middleware;

    public GlobalExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _envMock = new Mock<IWebHostEnvironment>();

        _middleware = new GlobalExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext_WhenNoException()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleException_AndSetCorrectStatusCode()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _nextMock.Setup(n => n(context)).Throws(new KeyNotFoundException("Not found"));
        
        var services = new ServiceCollection();
        services.AddSingleton(_envMock.Object);
        context.RequestServices = services.BuildServiceProvider();
        
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        // We need a response stream that we can read back or just check the status code
        context.Response.Body = new MemoryStream();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
    }
}
