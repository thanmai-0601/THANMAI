using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IEndorsementRepositoryTests
{
    [Fact]
    public void IEndorsementRepository_IsInterface()
    {
        Assert.True(typeof(IEndorsementRepository).IsInterface);
    }
}
