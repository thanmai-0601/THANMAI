using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class ClaimDocumentTests
{
    [Fact]
    public void ClaimDocument_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var doc = new ClaimDocument();
        var fileName = "death_certificate.jpg";

        // Act
        doc.FileName = fileName;

        // Assert
        Assert.Equal(fileName, doc.FileName);
    }
}
