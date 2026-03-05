using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IAuthServiceTests
{
    [Fact]
    public void IAuthService_IsInterface()
    {
        Assert.True(typeof(IAuthService).IsInterface);
    }
}
