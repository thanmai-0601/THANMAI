using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class InvoiceStatusTests
{
    [Fact]
    public void InvoiceStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Generated", InvoiceStatus.Generated.ToString());
    }
}
