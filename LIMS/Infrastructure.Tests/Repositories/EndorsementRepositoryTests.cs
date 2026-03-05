using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class EndorsementRepositoryTests
{
    private DbContextOptions<AppDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddEndorsement()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("EndorsementRepo_Create"));
        var repo = new EndorsementRepository(context);
        var endorsement = new PolicyEndorsement { RequestedAt = DateTime.UtcNow, Status = EndorsementStatus.Requested };

        // Act
        await repo.CreateAsync(endorsement);

        // Assert
        Assert.Equal(1, await context.PolicyEndorsements.CountAsync());
    }

    [Fact]
    public async Task HasPendingEndorsementAsync_ShouldReturnTrue_WhenPending()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("EndorsementRepo_HasPending"));
        context.PolicyEndorsements.Add(new PolicyEndorsement { PolicyId = 1, Status = EndorsementStatus.UnderReview });
        await context.SaveChangesAsync();
        var repo = new EndorsementRepository(context);

        // Act
        var result = await repo.HasPendingEndorsementAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetPendingCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = new AppDbContext(CreateOptions("EndorsementRepo_Count"));
        context.PolicyEndorsements.Add(new PolicyEndorsement { Status = EndorsementStatus.Requested });
        context.PolicyEndorsements.Add(new PolicyEndorsement { Status = EndorsementStatus.Approved });
        await context.SaveChangesAsync();
        var repo = new EndorsementRepository(context);

        // Act
        var result = await repo.GetPendingCountAsync();

        // Assert
        Assert.Equal(1, result);
    }
}
