using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IJwtTokenServiceTests
{
    [Fact]
    public void IJwtTokenService_IsInterface()
    {
        Assert.True(typeof(IJwtTokenService).IsInterface);
    }
}
