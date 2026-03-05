using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IPlanServiceTests
{
    [Fact]
    public void IPlanService_IsInterface()
    {
        Assert.True(typeof(IPlanService).IsInterface);
    }
}
