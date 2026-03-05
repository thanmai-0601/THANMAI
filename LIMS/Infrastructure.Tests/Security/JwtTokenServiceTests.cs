using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsAComplexSecretKeyForTestingUsedForAntigravityLIMSProjectOnly123!");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("LIMS");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("LIMS-Users");

        _service = new JwtTokenService(configMock.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwt()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@ex.com", Role = UserRole.Customer };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("LIMS", jwtToken.Issuer);
    }
}
