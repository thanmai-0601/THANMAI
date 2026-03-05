using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class InvoiceTests
{
    [Fact]
    public void Invoice_Initialization_ShouldHaveDefaultStatus()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        Assert.Equal(InvoiceStatus.Generated, invoice.Status);
    }

    [Fact]
    public void Invoice_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var invoice = new Invoice();
        var amount = 1200m;

        // Act
        invoice.AmountDue = amount;

        // Assert
        Assert.Equal(amount, invoice.AmountDue);
    }
}
