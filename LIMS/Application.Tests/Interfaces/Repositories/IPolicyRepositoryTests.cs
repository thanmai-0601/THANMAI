using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IPolicyRepositoryTests
{
    [Fact]
    public void IPolicyRepository_IsInterface()
    {
        Assert.True(typeof(IPolicyRepository).IsInterface);
    }
}
