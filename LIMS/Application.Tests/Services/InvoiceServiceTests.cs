using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepo;
    private readonly Mock<IPolicyRepository> _policyRepo;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _invoiceRepo = new Mock<IInvoiceRepository>();
        _policyRepo = new Mock<IPolicyRepository>();
        _service = new InvoiceService(_invoiceRepo.Object, _policyRepo.Object);
    }

    [Fact]
    public async Task GenerateScheduleAsync_ShouldCreateCorrectNumberOfInvoices_ForAnnual()
    {
        // Arrange
        var policy = new Policy { Id = 1, TenureYears = 5, PremiumAmount = 1000, PolicyNumber = "P1" };
        _policyRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(policy);
        _invoiceRepo.Setup(r => r.GenerateInvoiceNumberAsync()).ReturnsAsync("INV-20240001");

        // Act
        await _service.GenerateScheduleAsync(1, PaymentFrequency.Annual);

        // Assert
        _invoiceRepo.Verify(r => r.CreateRangeAsync(It.Is<List<Invoice>>(list => list.Count == 5)), Times.Once);
    }

    [Fact]
    public async Task GetScheduleAsync_ShouldReturnSchedule_WhenInvoicesExist()
    {
        // Arrange
        var policy = new Policy { Id = 1, PolicyNumber = "P1" };
        var invoices = new List<Invoice> { new Invoice { PolicyId = 1, AmountDue = 100, Frequency = PaymentFrequency.Monthly } };
        _policyRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(policy);
        _invoiceRepo.Setup(r => r.GetByPolicyIdAsync(1)).ReturnsAsync(invoices);

        // Act
        var result = await _service.GetScheduleAsync(1);

        // Assert
        Assert.Equal("P1", result.PolicyNumber);
        Assert.Equal(100, result.InstallmentAmount);
    }
}
