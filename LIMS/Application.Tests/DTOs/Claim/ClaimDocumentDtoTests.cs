using Application.DTOs.Claim;
using Xunit;

namespace Application.Tests.DTOs.Claim;

public class ClaimDocumentDtoTests
{
    [Fact]
    public void ClaimDocumentDto_PropertyAccessors_Work()
    {
        var dto = new ClaimDocumentDto { DocumentType = "T", FileName = "F" };
        Assert.Equal("T", dto.DocumentType);
        Assert.Equal("F", dto.FileName);
    }
}
