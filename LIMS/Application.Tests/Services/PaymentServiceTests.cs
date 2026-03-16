using Application.DTOs.Payment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;

using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo;
    private readonly Mock<IInvoiceRepository> _invoiceRepo;
    private readonly Mock<IPolicyRepository> _policyRepo;
    private readonly Mock<ICommissionRepository> _commissionRepo;
    private readonly Mock<IEmailService> _emailService;
    private readonly PaymentService _service;


    public PaymentServiceTests()
    {
        _paymentRepo = new Mock<IPaymentRepository>();
        _invoiceRepo = new Mock<IInvoiceRepository>();
        _policyRepo = new Mock<IPolicyRepository>();
        _commissionRepo = new Mock<ICommissionRepository>();
        _emailService = new Mock<IEmailService>();
        _service = new PaymentService(_paymentRepo.Object, _invoiceRepo.Object, _policyRepo.Object, _commissionRepo.Object, _emailService.Object);
    }


    [Fact]
    public async Task MakePaymentAsync_ShouldSucceed_AndActivatePolicy()
    {
        // Arrange
        var invoice = new Invoice { Id = 1, PolicyId = 1, AmountDue = 500, Status = InvoiceStatus.Pending, Policy = new Policy { Id = 1, CustomerId = 1, Status = PolicyStatus.Approved, TenureYears = 5, InsurancePlan = new InsurancePlan { PlanType = PlanType.TermLife } } };
        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(invoice);
        _paymentRepo.Setup(r => r.InvoiceAlreadyPaidAsync(1)).ReturnsAsync(false);

        var dto = new MakePaymentDto { InvoiceId = 1, PaymentMethod = "Card" };

        // Act
        var result = await _service.MakePaymentAsync(1, dto);

        // Assert
        Assert.Equal(PaymentStatus.Paid.ToString(), result.Status);
        _invoiceRepo.Verify(r => r.UpdateAsync(It.Is<Invoice>(i => i.Status == InvoiceStatus.Paid)), Times.Once);
        _policyRepo.Verify(r => r.UpdateAsync(It.Is<Policy>(p => p.Status == PolicyStatus.Active)), Times.Once);
    }

    [Fact]
    public async Task MakePaymentAsync_ShouldThrow_WhenInvoiceAlreadyPaid()
    {
        // Arrange
        var invoice = new Invoice { Id = 1, Policy = new Policy { CustomerId = 1 } };
        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(invoice);
        _paymentRepo.Setup(r => r.InvoiceAlreadyPaidAsync(1)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MakePaymentAsync(1, new MakePaymentDto { InvoiceId = 1 }));
    }
}
