using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class PolicyStatusTests
{
    [Fact]
    public void PolicyStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Active", PolicyStatus.Active.ToString());
    }
}
