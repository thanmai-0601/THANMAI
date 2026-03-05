using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class ClaimStatusTests
{
    [Fact]
    public void ClaimStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Submitted", ClaimStatus.Submitted.ToString());
    }
}
