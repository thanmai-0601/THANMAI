using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class PolicyEndorsementTests
{
    [Fact]
    public void PolicyEndorsement_Initialization_ShouldHaveDefaultStatus()
    {
        // Arrange & Act
        var endorsement = new PolicyEndorsement();

        // Assert
        Assert.Equal(EndorsementStatus.Requested, endorsement.Status);
    }

    [Fact]
    public void PolicyEndorsement_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var endorsement = new PolicyEndorsement();
        var type = EndorsementType.AddressChange;
        var json = "{\"address\": \"new\"}";

        // Act
        endorsement.Type = type;
        endorsement.ChangeRequestJson = json;

        // Assert
        Assert.Equal(type, endorsement.Type);
        Assert.Equal(json, endorsement.ChangeRequestJson);
    }
}
