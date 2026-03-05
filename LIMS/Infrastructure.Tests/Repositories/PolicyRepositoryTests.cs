using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class PolicyRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPolicy()
    {
        using var context = new AppDbContext(CreateOptions("PolicyRepo_GetById"));
        context.Policies.Add(new Policy { Id = 1, PolicyNumber = "P1" });
        await context.SaveChangesAsync();
        var repo = new PolicyRepository(context);

        var result = await repo.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("P1", result.PolicyNumber);
    }
}
