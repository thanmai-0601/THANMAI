using Application.Interfaces.Repositories;
using Xunit;

namespace Application.Tests.Interfaces.Repositories;

public class IInvoiceRepositoryTests
{
    [Fact]
    public void IInvoiceRepository_IsInterface()
    {
        Assert.True(typeof(IInvoiceRepository).IsInterface);
    }
}
