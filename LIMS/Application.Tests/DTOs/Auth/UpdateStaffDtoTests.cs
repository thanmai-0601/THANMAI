using Application.DTOs.Auth;
using Domain.Enums;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class UpdateStaffDtoTests
{
    [Fact]
    public void UpdateStaffDto_PropertyAccessors_Work()
    {
        var dto = new UpdateStaffDto { FullName = "F", Email = "E", PhoneNumber = "123", Role = UserRole.Agent };
        Assert.Equal("F", dto.FullName);
        Assert.Equal("E", dto.Email);
        Assert.Equal("123", dto.PhoneNumber);
        Assert.Equal(UserRole.Agent, dto.Role);
    }
}
