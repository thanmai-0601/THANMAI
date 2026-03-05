using Application.DTOs.Endorsement;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace API.Tests.Controllers;

public class EndorsementControllerTests
{
    private readonly Mock<IEndorsementService> _endorsementServiceMock;
    private readonly EndorsementController _controller;

    public EndorsementControllerTests()
    {
        _endorsementServiceMock = new Mock<IEndorsementService>();
        _controller = new EndorsementController(_endorsementServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Customer")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task RequestEndorsement_ShouldReturnOk_ForAddressChange()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new { NewAddress = "New Address", PolicyId = 10 });
        _endorsementServiceMock.Setup(s => s.RequestAddressChangeAsync(It.IsAny<int>(), It.IsAny<RequestAddressChangeDto>()))
            .ReturnsAsync(new EndorsementResponseDto { EndorsementId = 1 });

        // Act
        var result = await _controller.RequestEndorsement("address", payload);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((EndorsementResponseDto)okResult.Value!).EndorsementId);
    }
}
