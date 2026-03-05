using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class NomineeRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddNominees()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("NomineeRepo_AddRange"));
        var repo = new NomineeRepository(context);
        var nominees = new List<Nominee>
        {
            new Nominee { FullName = "N1", PolicyId = 1 },
            new Nominee { FullName = "N2", PolicyId = 1 }
        };

        // Act
        await repo.AddRangeAsync(nominees);

        // Assert
        Assert.Equal(2, await context.Nominees.CountAsync());
    }

    [Fact]
    public async Task DeleteByPolicyIdAsync_ShouldRemoveNominees()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("NomineeRepo_Delete"));
        context.Nominees.Add(new Nominee { PolicyId = 1, FullName = "N1" });
        context.Nominees.Add(new Nominee { PolicyId = 2, FullName = "N2" });
        await context.SaveChangesAsync();
        var repo = new NomineeRepository(context);

        // Act
        await repo.DeleteByPolicyIdAsync(1);

        // Assert
        Assert.Equal(1, await context.Nominees.CountAsync());
        Assert.Null(await context.Nominees.FirstOrDefaultAsync(n => n.PolicyId == 1));
    }
}
