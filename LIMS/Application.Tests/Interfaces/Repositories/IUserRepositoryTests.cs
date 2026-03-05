using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IUserRepositoryTests
{
    [Fact]
    public void IUserRepository_IsInterface()
    {
        Assert.True(typeof(IUserRepository).IsInterface);
    }
}
