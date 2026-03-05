using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class StaffResponseDtoTests
{
    [Fact]
    public void StaffResponseDto_PropertyAccessors_Work()
    {
        var dto = new StaffResponseDto 
        { 
            UserId = 1, 
            FullName = "F", 
            Email = "E", 
            Role = "R", 
            TemporaryPassword = "P" 
        };
        Assert.Equal(1, dto.UserId);
        Assert.Equal("F", dto.FullName);
        Assert.Equal("E", dto.Email);
        Assert.Equal("R", dto.Role);
        Assert.Equal("P", dto.TemporaryPassword);
    }
}
