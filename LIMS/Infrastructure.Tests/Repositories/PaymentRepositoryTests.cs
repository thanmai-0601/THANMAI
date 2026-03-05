using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class PaymentRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPayment()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PaymentRepo_Create"));
        var repo = new PaymentRepository(context);
        var payment = new Payment { AmountPaid = 1000, Status = PaymentStatus.Paid, PaymentDate = DateTime.UtcNow };

        // Act
        await repo.CreateAsync(payment);

        // Assert
        Assert.Equal(1, await context.Payments.CountAsync());
    }

    [Fact]
    public async Task GetByPolicyIdAsync_ShouldReturnPayments()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PaymentRepo_GetByPolicy"));

        // Seed required related entities for Include(p => p.Invoice) to work
        var customer = new User
        {
            FullName = "Test Customer",
            Email = "cust@test.com",
            PasswordHash = "hash",
            PhoneNumber = "1234567890",
            Role = UserRole.Customer
        };
        context.Users.Add(customer);
        await context.SaveChangesAsync();

        var plan = new InsurancePlan
        {
            PlanName = "Test Plan",
            Description = "Desc",
            MinSumAssured = 100000,
            MaxSumAssured = 500000,
            TenureOptions = "10,20",
            MinEntryAge = 18,
            MaxEntryAge = 60,
            MinAnnualIncome = 300000,
            AvailableRiders = "",
            BaseRatePer1000 = 5,
            LowRiskMultiplier = 1,
            MediumRiskMultiplier = 1.25m,
            HighRiskMultiplier = 1.6m,
            CommissionPercentage = 5
        };
        context.InsurancePlans.Add(plan);
        await context.SaveChangesAsync();

        var policy1 = new Policy
        {
            PolicyNumber = "POL-0001",
            CustomerId = customer.Id,
            InsurancePlanId = plan.Id,
            SumAssured = 200000,
            TenureYears = 10,
            Status = PolicyStatus.Active
        };
        var policy2 = new Policy
        {
            PolicyNumber = "POL-0002",
            CustomerId = customer.Id,
            InsurancePlanId = plan.Id,
            SumAssured = 300000,
            TenureYears = 20,
            Status = PolicyStatus.Active
        };
        context.Policies.AddRange(policy1, policy2);
        await context.SaveChangesAsync();

        var invoice1 = new Invoice
        {
            PolicyId = policy1.Id,
            InvoiceNumber = "INV-0001",
            AmountDue = 500,
            DueDate = DateTime.UtcNow,
            Frequency = PaymentFrequency.Annual,
            PeriodYear = 2026,
            PeriodNumber = 1,
            Status = InvoiceStatus.Paid
        };
        var invoice2 = new Invoice
        {
            PolicyId = policy2.Id,
            InvoiceNumber = "INV-0002",
            AmountDue = 1000,
            DueDate = DateTime.UtcNow,
            Frequency = PaymentFrequency.Annual,
            PeriodYear = 2026,
            PeriodNumber = 1,
            Status = InvoiceStatus.Paid
        };
        context.Invoices.AddRange(invoice1, invoice2);
        await context.SaveChangesAsync();

        context.Payments.Add(new Payment
        {
            PolicyId = policy1.Id,
            InvoiceId = invoice1.Id,
            AmountPaid = 500,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        });
        context.Payments.Add(new Payment
        {
            PolicyId = policy2.Id,
            InvoiceId = invoice2.Id,
            AmountPaid = 1000,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new PaymentRepository(context);

        // Act
        var result = await repo.GetByPolicyIdAsync(policy1.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(500, result[0].AmountPaid);
    }

    [Fact]
    public async Task InvoiceAlreadyPaidAsync_ShouldReturnTrueIfPaid()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PaymentRepo_AlreadyPaid"));
        context.Payments.Add(new Payment { InvoiceId = 1, Status = PaymentStatus.Paid });
        await context.SaveChangesAsync();
        var repo = new PaymentRepository(context);

        // Act
        var result = await repo.InvoiceAlreadyPaidAsync(1);

        // Assert
        Assert.True(result);
    }
}
