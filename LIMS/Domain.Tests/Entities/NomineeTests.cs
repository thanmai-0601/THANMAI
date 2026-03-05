using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class NomineeTests
{
    [Fact]
    public void Nominee_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var nominee = new Nominee();
        var name = "Jane Doe";
        var relation = "Spouse";
        var percentage = 100m;

        // Act
        nominee.FullName = name;
        nominee.Relationship = relation;
        nominee.AllocationPercentage = percentage;

        // Assert
        Assert.Equal(name, nominee.FullName);
        Assert.Equal(relation, nominee.Relationship);
        Assert.Equal(percentage, nominee.AllocationPercentage);
    }

    [Fact]
    public void Nominee_AllocationPercentage_ShouldHandleZero()
    {
        // Arrange
        var nominee = new Nominee { AllocationPercentage = 0m };

        // Act & Assert
        Assert.Equal(0m, nominee.AllocationPercentage);
    }
}
