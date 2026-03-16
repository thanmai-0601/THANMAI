using Application.DTOs.Claim;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace API.Tests.Controllers;

public class ClaimControllerTests
{
    private readonly Mock<IClaimService> _claimServiceMock;
    private readonly Mock<IPdfValidationService> _pdfValidationMock;
    private readonly ClaimController _controller;

    public ClaimControllerTests()
    {
        _claimServiceMock = new Mock<IClaimService>();
        _pdfValidationMock = new Mock<IPdfValidationService>();
        _controller = new ClaimController(_claimServiceMock.Object, _pdfValidationMock.Object);

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
    public async Task RaiseClaim_ShouldReturnOk_WithClaimResponse()
    {
        // Arrange
        var dto = new RaiseClaimDto { PolicyNumber = "POL-1" };
        var response = new ClaimResponseDto { ClaimNumber = "CLM-1" };
        _claimServiceMock.Setup(s => s.RaiseClaimAsync(dto)).ReturnsAsync(response);

        // Act
        var result = await _controller.RaiseClaim(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("CLM-1", ((ClaimResponseDto)okResult.Value!).ClaimNumber);
    }
}
