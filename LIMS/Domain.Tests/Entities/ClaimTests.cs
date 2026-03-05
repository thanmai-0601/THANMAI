using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class ClaimTests
{
    [Fact]
    public void Claim_Initialization_ShouldHaveDefaultStatus()
    {
        // Arrange & Act
        var claim = new Claim();

        // Assert
        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.NotNull(claim.ClaimDocuments);
    }

    [Fact]
    public void Claim_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var claim = new Claim();
        var amount = 50000m;
        var reason = "Natural Death";

        // Act
        claim.ClaimAmount = amount;
        claim.ClaimReason = reason;

        // Assert
        Assert.Equal(amount, claim.ClaimAmount);
        Assert.Equal(reason, claim.ClaimReason);
    }
}
