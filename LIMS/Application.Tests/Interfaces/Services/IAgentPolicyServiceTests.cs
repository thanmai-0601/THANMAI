using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IAgentPolicyServiceTests
{
    [Fact]
    public void IAgentPolicyService_IsInterface()
    {
        Assert.True(typeof(IAgentPolicyService).IsInterface);
    }
}
