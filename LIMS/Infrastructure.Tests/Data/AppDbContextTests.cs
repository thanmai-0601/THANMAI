using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Data;

public class AppDbContextTests
{
    private AppDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void AppDbContext_CanSaveAndRetrieveUser()
    {
        // Arrange
        using var context = GetContext();
        var user = new User { FullName = "Test User", Email = "test@example.com", PasswordHash = "hash" };

        // Act
        context.Users.Add(user);
        context.SaveChanges();

        // Assert
        var retrievedUser = context.Users.FirstOrDefault(u => u.Email == "test@example.com");
        Assert.NotNull(retrievedUser);
        Assert.Equal("Test User", retrievedUser.FullName);
    }

    [Fact]
    public void AppDbContext_CanSaveAndRetrievePolicy()
    {
        // Arrange
        using var context = GetContext();
        var plan = new InsurancePlan { PlanName = "Life Plan", TenureOptions = "10,20" };
        var user = new User { FullName = "Customer", Email = "cust@example.com", PasswordHash = "hash" };
        context.InsurancePlans.Add(plan);
        context.Users.Add(user);
        context.SaveChanges();

        var policy = new Policy 
        { 
            PolicyNumber = "POL-001", 
            CustomerId = user.Id, 
            InsurancePlanId = plan.Id,
            SumAssured = 100000
        };

        // Act
        context.Policies.Add(policy);
        context.SaveChanges();

        // Assert
        var retrievedPolicy = context.Policies.Include(p => p.Customer).FirstOrDefault(p => p.PolicyNumber == "POL-001");
        Assert.NotNull(retrievedPolicy);
        Assert.Equal("Customer", retrievedPolicy.Customer.FullName);
    }
}
