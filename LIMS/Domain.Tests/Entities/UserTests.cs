using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_Initialization_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.True(user.IsActive);
        Assert.False(user.IsDeleted);
        Assert.False(user.MustChangePassword);
        Assert.Empty(user.FullName);
        Assert.Empty(user.Email);
        Assert.NotNull(user.CustomerPolicies);
        Assert.NotNull(user.AgentPolicies);
    }

    [Fact]
    public void User_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var user = new User();
        var email = "test@example.com";
        var fullName = "John Doe";
        var role = UserRole.Customer;

        // Act
        user.Email = email;
        user.FullName = fullName;
        user.Role = role;

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void User_SoftDelete_ShouldUpdateFlag()
    {
        // Arrange
        var user = new User();
        var deletedAt = DateTime.UtcNow;

        // Act
        user.IsDeleted = true;
        user.DeletedAt = deletedAt;

        // Assert
        Assert.True(user.IsDeleted);
        Assert.Equal(deletedAt, user.DeletedAt);
    }
}
