using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class PlanRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActivePlans_ByDefault()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PlanRepo_GetAllActive"));
        context.InsurancePlans.Add(new InsurancePlan { Id = 1, PlanName = "P1", IsActive = true });
        context.InsurancePlans.Add(new InsurancePlan { Id = 2, PlanName = "P2", IsActive = false });
        await context.SaveChangesAsync();
        var repo = new PlanRepository(context);

        // Act
        var result = await repo.GetAllAsync(includeInactive: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("P1", result[0].PlanName);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPlan()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PlanRepo_Create"));
        var repo = new PlanRepository(context);
        var plan = new InsurancePlan { PlanName = "New Plan", IsActive = true };

        // Act
        await repo.CreateAsync(plan);

        // Assert
        Assert.Equal(1, await context.InsurancePlans.CountAsync());
    }

    [Fact]
    public async Task PlanNameExistsAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("PlanRepo_Exists"));
        context.InsurancePlans.Add(new InsurancePlan { PlanName = "Test Plan" });
        await context.SaveChangesAsync();
        var repo = new PlanRepository(context);

        // Act
        var result = await repo.PlanNameExistsAsync("test plan");

        // Assert
        Assert.True(result);
    }
}
