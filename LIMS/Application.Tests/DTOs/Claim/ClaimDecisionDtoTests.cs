using Application.DTOs.Claim;
using Xunit;

namespace Application.Tests.DTOs.Claim;

public class ClaimDecisionDtoTests
{
    [Fact]
    public void ClaimDecisionDto_PropertyAccessors_Work()
    {
        var dto = new ClaimDecisionDto 
        { 
            IsApproved = true, 
            SettledAmount = 5000, 
            OfficerRemarks = "OR", 
            RejectionReason = "RR" 
        };
        Assert.True(dto.IsApproved);
        Assert.Equal(5000, dto.SettledAmount);
        Assert.Equal("OR", dto.OfficerRemarks);
        Assert.Equal("RR", dto.RejectionReason);
    }
}
