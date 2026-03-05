using Application.DTOs.Claim;
using Xunit;

namespace Application.Tests.DTOs.Claim;

public class ClaimResponseDtoTests
{
    [Fact]
    public void ClaimResponseDto_PropertyAccessors_Work()
    {
        var dto = new ClaimResponseDto { ClaimId = 1, Status = "S" };
        Assert.Equal(1, dto.ClaimId);
        Assert.Equal("S", dto.Status);
    }
}
