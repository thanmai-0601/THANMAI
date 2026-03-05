using API.Controllers;
using Application.DTOs.Payment;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _controller = new PaymentController(_paymentServiceMock.Object, _invoiceServiceMock.Object);
    }

    [Fact]
    public async Task GetPaymentInfo_ShouldReturnHistory_ByDefault()
    {
        // Act
        var result = await _controller.GetPaymentInfo(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _paymentServiceMock.Verify(s => s.GetPaymentHistoryAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetPaymentInfo_ShouldReturnSchedule_WhenRequested()
    {
        // Act
        var result = await _controller.GetPaymentInfo(1, "Schedule");

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _invoiceServiceMock.Verify(s => s.GetScheduleAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetInvoice_ShouldReturnOk()
    {
        // Arrange
        _invoiceServiceMock.Setup(s => s.GetInvoiceAsync(10)).ReturnsAsync(new InvoiceResponseDto());

        // Act
        var result = await _controller.GetInvoice(10);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
