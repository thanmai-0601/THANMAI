using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class InvoiceRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyInvoice()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("InvoiceRepo_Update"));
        var invoice = new Invoice { Id = 1, InvoiceNumber = "INV-01", Status = InvoiceStatus.Generated };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        var repo = new InvoiceRepository(context);

        // Act
        invoice.Status = InvoiceStatus.Paid;
        await repo.UpdateAsync(invoice);

        // Assert
        var updated = await context.Invoices.FindAsync(1);
        Assert.Equal(InvoiceStatus.Paid, updated!.Status);
    }

    [Fact]
    public async Task GetByPolicyIdAsync_ShouldReturnInvoices()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("InvoiceRepo_GetByPolicy"));
        context.Invoices.Add(new Invoice { PolicyId = 1, InvoiceNumber = "I1" });
        context.Invoices.Add(new Invoice { PolicyId = 2, InvoiceNumber = "I2" });
        await context.SaveChangesAsync();
        var repo = new InvoiceRepository(context);

        // Act
        var result = await repo.GetByPolicyIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("I1", result[0].InvoiceNumber);
    }

    [Fact]
    public async Task GenerateInvoiceNumberAsync_ShouldReturnNextNumber()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("InvoiceRepo_GenerateNum"));
        var year = DateTime.UtcNow.Year;
        context.Invoices.Add(new Invoice { InvoiceNumber = $"INV-{year}0001" });
        await context.SaveChangesAsync();
        var repo = new InvoiceRepository(context);

        // Act
        var result = await repo.GenerateInvoiceNumberAsync();

        // Assert
        Assert.Equal($"INV-{year}0002", result);
    }
}
