using Application.DTOs.Endorsement;
using Xunit;

namespace Application.Tests.DTOs.Endorsement;

public class EndorsementResponseDtoTests
{
    [Fact]
    public void EndorsementResponseDto_PropertyAccessors_Work()
    {
        var dto = new EndorsementResponseDto { EndorsementId = 1, PolicyNumber = "P" };
        Assert.Equal(1, dto.EndorsementId);
        Assert.Equal("P", dto.PolicyNumber);
    }
}
