using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class PaymentFrequencyTests
{
    [Fact]
    public void PaymentFrequency_ShouldHaveExpectedValues()
    {
        Assert.Equal("Annual", PaymentFrequency.Annual.ToString());
    }
}
