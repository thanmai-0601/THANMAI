using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class EndorsementTypeTests
{
    [Fact]
    public void EndorsementType_ShouldHaveExpectedValues()
    {
        Assert.Equal("AddressChange", EndorsementType.AddressChange.ToString());
    }
}
