using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class ChangePasswordDtoTests
{
    [Fact]
    public void ChangePasswordDto_PropertyAccessors_Work()
    {
        var dto = new ChangePasswordDto 
        { 
            CurrentPassword = "C", 
            NewPassword = "N12345678!", 
            ConfirmPassword = "N12345678!" 
        };
        Assert.Equal("C", dto.CurrentPassword);
        Assert.Equal("N12345678!", dto.NewPassword);
    }
}
