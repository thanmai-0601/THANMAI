using Infrastructure.Security;
using Xunit;

namespace Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnHashedString()
    {
        // Arrange
        var password = "SafePassword123";

        // Act
        var hash = _hasher.Hash(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "SafePassword123";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "SafePassword123";
        var wrongPassword = "WrongPassword123";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }
}
