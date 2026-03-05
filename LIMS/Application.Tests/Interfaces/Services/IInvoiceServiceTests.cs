using Application.Interfaces.Services;
using Xunit;

namespace Application.Tests.Interfaces.Services;

public class IInvoiceServiceTests
{
    [Fact]
    public void IInvoiceService_IsInterface()
    {
        Assert.True(typeof(IInvoiceService).IsInterface);
    }
}
