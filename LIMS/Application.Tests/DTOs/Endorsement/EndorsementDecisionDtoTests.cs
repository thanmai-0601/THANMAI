using Application.DTOs.Endorsement;
using Xunit;

namespace Application.Tests.DTOs.Endorsement;

public class EndorsementDecisionDtoTests
{
    [Fact]
    public void EndorsementDecisionDto_PropertyAccessors_Work()
    {
        var dto = new EndorsementDecisionDto 
        { 
            IsApproved = true, 
            AgentRemarks = "AR", 
            RejectionReason = "RR" 
        };
        Assert.True(dto.IsApproved);
        Assert.Equal("AR", dto.AgentRemarks);
        Assert.Equal("RR", dto.RejectionReason);
    }
}
