using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IPaymentRepositoryTests
{
    [Fact]
    public void IPaymentRepository_IsInterface()
    {
        Assert.True(typeof(IPaymentRepository).IsInterface);
    }
}
