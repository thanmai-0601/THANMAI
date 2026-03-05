using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class INotificationServiceTests
{
    [Fact]
    public void INotificationService_IsInterface()
    {
        Assert.True(typeof(INotificationService).IsInterface);
    }
}
