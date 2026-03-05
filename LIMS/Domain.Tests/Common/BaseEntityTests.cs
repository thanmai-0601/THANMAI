using Domain.Common;
using Xunit;

namespace Domain.Tests.Common;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void BaseEntity_ShouldHaveWorkingIdProperty()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.Id = 100;

        // Assert
        Assert.Equal(100, entity.Id);
    }
}
