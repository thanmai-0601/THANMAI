using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IPremiumCalculationServiceTests
{
    [Fact]
    public void IPremiumCalculationService_IsInterface()
    {
        Assert.True(typeof(IPremiumCalculationService).IsInterface);
    }
}
