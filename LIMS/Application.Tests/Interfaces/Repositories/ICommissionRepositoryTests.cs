using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class ICommissionRepositoryTests
{
    [Fact]
    public void ICommissionRepository_IsInterface()
    {
        Assert.True(typeof(ICommissionRepository).IsInterface);
    }
}
