using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class INotificationRepositoryTests
{
    [Fact]
    public void INotificationRepository_IsInterface()
    {
        Assert.True(typeof(INotificationRepository).IsInterface);
    }
}
