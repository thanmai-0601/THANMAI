using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Tests.Repositories;

public class CommissionRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task GetCommissionsByAgentAsync_ShouldReturnCommissions()
    {
        using var context = new AppDbContext(CreateOptions("CommRepo_GetByAgent"));

        // Seed required related entities for Include(c => c.Policy) to work
        var agent = new User
        {
            FullName = "Test Agent",
            Email = "agent@test.com",
            PasswordHash = "hash",
            PhoneNumber = "1234567890",
            Role = UserRole.Agent
        };
        context.Users.Add(agent);
        await context.SaveChangesAsync();

        var customer = new User
        {
            FullName = "Test Customer",
            Email = "cust@test.com",
            PasswordHash = "hash",
            PhoneNumber = "0987654321",
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
            AgentId = agent.Id,
            SumAssured = 200000,
            TenureYears = 10,
            Status = PolicyStatus.Active
        };
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var commission = new Commission
        {
            AgentId = agent.Id,
            PolicyId = policy.Id,
            EarnedOn = DateTime.UtcNow,
            PremiumAmount = 1000,
            CommissionPercentage = 5,
            CommissionAmount = 100
        };
        context.Commissions.Add(commission);
        await context.SaveChangesAsync();

        var repo = new CommissionRepository(context);
        var result = await repo.GetByAgentIdAsync(agent.Id);

        Assert.NotEmpty(result);
    }
}
