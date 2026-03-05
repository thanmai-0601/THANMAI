using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class LoginDtoTests
{
    [Fact]
    public void LoginDto_PropertyAccessors_Work()
    {
        var dto = new LoginDto { Email = "E", Password = "P" };
        Assert.Equal("E", dto.Email);
        Assert.Equal("P", dto.Password);
    }
}
