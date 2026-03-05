using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class EndorsementStatusTests
{
    [Fact]
    public void EndorsementStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Requested", EndorsementStatus.Requested.ToString());
    }
}
