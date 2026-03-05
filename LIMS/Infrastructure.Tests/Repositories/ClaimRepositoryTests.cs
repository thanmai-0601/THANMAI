using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infrastructure.Tests.Repositories;

public class ClaimRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddClaim()
    {
        using var context = new AppDbContext(CreateOptions("ClaimRepo_Create"));
        var repo = new ClaimRepository(context);
        var claim = new Claim { ClaimNumber = "CLM-1", Status = ClaimStatus.Submitted };

        var result = await repo.CreateAsync(claim);

        Assert.Equal(1, await context.Claims.CountAsync());
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ShouldReturnCorrectClaims()
    {
        using var context = new AppDbContext(CreateOptions("ClaimRepo_GetByCustomer"));

        // Seed required related entities for Include/ThenInclude to work
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

        var policy = new Policy
        {
            PolicyNumber = "POL-0001",
            CustomerId = customer.Id,
            InsurancePlanId = plan.Id,
            SumAssured = 200000,
            TenureYears = 10,
            Status = PolicyStatus.Active
        };
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var claim = new Claim
        {
            CustomerId = customer.Id,
            PolicyId = policy.Id,
            ClaimNumber = "C1",
            Status = ClaimStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        var repo = new ClaimRepository(context);
        var result = await repo.GetByCustomerIdAsync(customer.Id);

        Assert.NotEmpty(result);
        Assert.Equal("C1", result[0].ClaimNumber);
    }
}
