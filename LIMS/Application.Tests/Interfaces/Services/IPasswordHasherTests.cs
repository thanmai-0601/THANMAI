using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IPasswordHasherTests
{
    [Fact]
    public void IPasswordHasher_IsInterface()
    {
        Assert.True(typeof(IPasswordHasher).IsInterface);
    }
}
