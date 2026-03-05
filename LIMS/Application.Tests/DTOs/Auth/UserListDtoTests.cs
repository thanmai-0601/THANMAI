using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class UserListDtoTests
{
    [Fact]
    public void UserListDto_PropertyAccessors_Work()
    {
        var dto = new UserListDto 
        { 
            UserId = 1, 
            FullName = "F", 
            Email = "E", 
            Role = "R", 
            IsActive = true,
            PhoneNumber = "123"
        };
        Assert.Equal(1, dto.UserId);
        Assert.Equal("F", dto.FullName);
        Assert.Equal("E", dto.Email);
        Assert.Equal("R", dto.Role);
        Assert.True(dto.IsActive);
        Assert.Equal("123", dto.PhoneNumber);
    }
}
