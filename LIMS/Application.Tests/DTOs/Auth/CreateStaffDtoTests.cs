using Application.DTOs.Auth;
using Xunit;

namespace Application.Tests.DTOs.Auth;

public class CreateStaffDtoTests
{
    [Fact]
    public void CreateStaffDto_PropertyAccessors_Work()
    {
        var dto = new CreateStaffDto { Email = "E", FullName = "F", PhoneNumber = "123", Password = "P" };
        Assert.Equal("E", dto.Email);
        Assert.Equal("F", dto.FullName);
        Assert.Equal("123", dto.PhoneNumber);
        Assert.Equal("P", dto.Password);
    }
}
