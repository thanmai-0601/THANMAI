using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IDashboardServiceTests
{
    [Fact]
    public void IDashboardService_IsInterface()
    {
        Assert.True(typeof(IDashboardService).IsInterface);
    }
}
