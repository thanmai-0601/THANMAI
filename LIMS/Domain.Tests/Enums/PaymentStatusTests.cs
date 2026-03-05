using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class PaymentStatusTests
{
    [Fact]
    public void PaymentStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Paid", PaymentStatus.Paid.ToString());
    }
}
