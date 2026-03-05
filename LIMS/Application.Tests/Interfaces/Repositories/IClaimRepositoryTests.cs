using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IClaimRepositoryTests
{
    [Fact]
    public void IClaimRepository_IsInterface()
    {
        Assert.True(typeof(IClaimRepository).IsInterface);
    }
}
