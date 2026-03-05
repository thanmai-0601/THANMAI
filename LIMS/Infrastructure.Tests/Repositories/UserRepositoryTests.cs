using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        using var context = new AppDbContext(CreateOptions("UserRepo_GetByEmail"));
        context.Users.Add(new User { Email = "test@example.com", FullName = "Test", PasswordHash = "H", Role = UserRole.Customer });
        await context.SaveChangesAsync();
        var repo = new UserRepository(context);

        var result = await repo.GetByEmailAsync("test@example.com");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }
}
