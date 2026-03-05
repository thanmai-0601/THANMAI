using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IPolicyServiceTests
{
    [Fact]
    public void IPolicyService_IsInterface()
    {
        Assert.True(typeof(IPolicyService).IsInterface);
    }
}
