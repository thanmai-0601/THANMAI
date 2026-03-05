using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IClaimServiceTests
{
    [Fact]
    public void IClaimService_IsInterface()
    {
        Assert.True(typeof(IClaimService).IsInterface);
    }
}
