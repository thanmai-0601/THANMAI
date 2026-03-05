using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IEndorsementServiceTests
{
    [Fact]
    public void IEndorsementService_IsInterface()
    {
        Assert.True(typeof(IEndorsementService).IsInterface);
    }
}
