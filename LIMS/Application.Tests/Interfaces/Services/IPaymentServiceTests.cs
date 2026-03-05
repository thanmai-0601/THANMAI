using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IPaymentServiceTests
{
    [Fact]
    public void IPaymentService_IsInterface()
    {
        Assert.True(typeof(IPaymentService).IsInterface);
    }
}
