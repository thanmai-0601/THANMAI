using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class ResetPasswordDtoTests
{
    [Fact]
    public void ResetPasswordDto_PropertyAccessors_Work()
    {
        var dto = new ResetPasswordDto { Email = "E", NewPassword = "P" };
        Assert.Equal("E", dto.Email);
        Assert.Equal("P", dto.NewPassword);
    }
}
