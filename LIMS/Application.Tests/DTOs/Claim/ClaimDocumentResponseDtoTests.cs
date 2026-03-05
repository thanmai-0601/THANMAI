using Application.DTOs.Claim;
using Xunit;

namespace Application.Tests.DTOs.Claim;

public class ClaimDocumentResponseDtoTests
{
    [Fact]
    public void ClaimDocumentResponseDto_PropertyAccessors_Work()
    {
        var dto = new ClaimDocumentResponseDto { DocumentId = 1, DocumentType = "T" };
        Assert.Equal(1, dto.DocumentId);
        Assert.Equal("T", dto.DocumentType);
    }
}
