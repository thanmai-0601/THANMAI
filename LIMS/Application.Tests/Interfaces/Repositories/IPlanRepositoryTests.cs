using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IPlanRepositoryTests
{
    [Fact]
    public void IPlanRepository_IsInterface()
    {
        Assert.True(typeof(IPlanRepository).IsInterface);
    }
}
