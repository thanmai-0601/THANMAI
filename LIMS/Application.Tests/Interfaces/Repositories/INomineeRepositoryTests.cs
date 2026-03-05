using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class INomineeRepositoryTests
{
    [Fact]
    public void INomineeRepository_IsInterface()
    {
        Assert.True(typeof(INomineeRepository).IsInterface);
    }
}
