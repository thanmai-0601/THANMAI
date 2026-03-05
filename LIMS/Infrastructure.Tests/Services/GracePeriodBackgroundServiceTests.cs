using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public class GracePeriodBackgroundServiceTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<GracePeriodBackgroundService>> _loggerMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;

    public GracePeriodBackgroundServiceTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<GracePeriodBackgroundService>>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _policyRepoMock = new Mock<IPolicyRepository>();

        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IInvoiceRepository))).Returns(_invoiceRepoMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IPolicyRepository))).Returns(_policyRepoMock.Object);
    }

    [Fact]
    public async Task ProcessOverdueInvoicesAsync_ShouldMoveToGrace_WhenPastDueDate()
    {
        // Arrange
        var service = new GracePeriodBackgroundServiceWrapper(_scopeFactoryMock.Object, _loggerMock.Object);
        var invoice = new Invoice { Id = 1, PolicyId = 1, Status = InvoiceStatus.Pending };
        var policy = new Policy { Id = 1, Status = PolicyStatus.Active };

        _invoiceRepoMock.Setup(r => r.GetOverdueInvoicesAsync()).ReturnsAsync(new List<Invoice> { invoice });
        _policyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policy);
        _invoiceRepoMock.Setup(r => r.GetGraceInvoicesAsync()).ReturnsAsync(new List<Invoice>());

        // Act
        await service.TriggerProcessOverdueInvoicesAsync();

        // Assert
        Assert.Equal(InvoiceStatus.Grace, invoice.Status);
        Assert.Equal(PolicyStatus.Grace, policy.Status);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(invoice), Times.Once);
        _policyRepoMock.Verify(r => r.UpdateAsync(policy), Times.Once);
    }
}

// Wrapper to access protected/private method for testing
public class GracePeriodBackgroundServiceWrapper : GracePeriodBackgroundService
{
    public GracePeriodBackgroundServiceWrapper(IServiceScopeFactory scopeFactory, ILogger<GracePeriodBackgroundService> logger) 
        : base(scopeFactory, logger) { }

    public Task TriggerProcessOverdueInvoicesAsync()
    {
        // Since ProcessOverdueInvoicesAsync is private, we'd normally need reflection or 
        // to make it internal and use InternalsVisibleTo. 
        // For the sake of this test task, I will use reflection or wrap the logic if possible.
        // Actually, looking at the code, ProcessOverdueInvoicesAsync is private.
        var method = typeof(GracePeriodBackgroundService).GetMethod("ProcessOverdueInvoicesAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Task)method.Invoke(this, null);
    }
}
