using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IAgentAssignmentServiceTests
{
    [Fact]
    public void IAgentAssignmentService_IsInterface()
    {
        Assert.True(typeof(IAgentAssignmentService).IsInterface);
    }
}
