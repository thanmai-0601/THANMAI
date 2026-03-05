using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class UserRoleTests
{
    [Fact]
    public void UserRole_ShouldHaveExpectedValues()
    {
        Assert.Equal("Admin", UserRole.Admin.ToString());
    }
}
