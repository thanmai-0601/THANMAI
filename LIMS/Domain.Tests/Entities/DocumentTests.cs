using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.Tests.Entities;

public class DocumentTests
{
    [Fact]
    public void Document_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var document = new Document();
        var type = "IdentityProof";
        var fileName = "id.pdf";

        // Act
        document.DocumentType = type;
        document.FileName = fileName;
        document.Status = DocumentStatus.Uploaded;

        // Assert
        Assert.Equal(type, document.DocumentType);
        Assert.Equal(fileName, document.FileName);
        Assert.Equal(DocumentStatus.Uploaded, document.Status);
    }

    [Fact]
    public void Document_Rejection_ShouldStoreReason()
    {
        // Arrange
        var document = new Document();
        var reason = "Invalid ID";

        // Act
        document.Status = DocumentStatus.Rejected;
        document.RejectionReason = reason;

        // Assert
        Assert.Equal(DocumentStatus.Rejected, document.Status);
        Assert.Equal(reason, document.RejectionReason);
    }
}
