using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class AuthResponseDtoTests
{
    [Fact]
    public void AuthResponseDto_PropertyAccessors_Work()
    {
        var dto = new AuthResponseDto { Token = "T", Role = "R", FullName = "F" };
        Assert.Equal("T", dto.Token);
        Assert.Equal("R", dto.Role);
        Assert.Equal("F", dto.FullName);
    }
}
