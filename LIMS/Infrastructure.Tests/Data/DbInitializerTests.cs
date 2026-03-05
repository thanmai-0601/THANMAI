using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Application.Interfaces.Services;
using Xunit;

namespace Infrastructure.Tests.Data;

public class DbInitializerTests
{
    [Fact]
    public void Seed_ShouldPopulateData_WhenDatabaseIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DbInitializerTest")
            .Options;

        using var context = new AppDbContext(options);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed_pass");

        // Act
        DbInitializer.Seed(context, passwordHasherMock.Object);

        // Assert
        Assert.True(context.Users.Any(u => u.Email == "admin@lims.com"));
        Assert.True(context.InsurancePlans.Any());
    }

    [Fact]
    public void Seed_ShouldNotDuplicateData_WhenDatabaseIsAlreadySeeded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DbInitializerTest_Duplicate")
            .Options;

        using var context = new AppDbContext(options);
        context.Users.Add(new User { Email = "admin@lims.com", PasswordHash = "...", Role = UserRole.Admin });
        context.SaveChanges();

        var initialUserCount = context.Users.Count();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        // Act
        DbInitializer.Seed(context, passwordHasherMock.Object);

        // Assert
        Assert.Equal(initialUserCount, context.Users.Count());
    }
}
