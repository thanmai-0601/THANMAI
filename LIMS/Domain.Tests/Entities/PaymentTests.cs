using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void Payment_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var payment = new Payment();
        var amount = 1200m;
        var method = "Card";
        var date = DateTime.UtcNow;

        // Act
        payment.AmountPaid = amount;
        payment.PaymentMethod = method;
        payment.PaymentDate = date;
        payment.Status = PaymentStatus.Paid;

        // Assert
        Assert.Equal(amount, payment.AmountPaid);
        Assert.Equal(method, payment.PaymentMethod);
        Assert.Equal(date, payment.PaymentDate);
        Assert.Equal(PaymentStatus.Paid, payment.Status);
    }
}
