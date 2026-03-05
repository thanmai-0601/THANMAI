using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class RegisterDtoTests
{
    [Fact]
    public void RegisterDto_PropertyAccessors_Work()
    {
        var dto = new RegisterDto { Email = "E", FullName = "F", Password = "P" };
        Assert.Equal("E", dto.Email);
        Assert.Equal("F", dto.FullName);
        Assert.Equal("P", dto.Password);
    }
}
