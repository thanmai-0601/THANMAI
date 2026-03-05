using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IClaimsOfficerAssignmentServiceTests
{
    [Fact]
    public void IClaimsOfficerAssignmentService_IsInterface()
    {
        Assert.True(typeof(IClaimsOfficerAssignmentService).IsInterface);
    }
}
