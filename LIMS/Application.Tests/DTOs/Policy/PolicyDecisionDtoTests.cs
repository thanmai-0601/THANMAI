using Application.DTOs.Policy;
using Xunit;

namespace Application.Tests.DTOs.Policy;

public class PolicyDecisionDtoTests
{
    [Fact]
    public void PolicyDecisionDto_PropertyAccessors_Work()
    {
        var dto = new PolicyDecisionDto 
        { 
            IsApproved = true, 
            RejectionReason = "RR", 
            RiskCategory = "Low", 
            AgentRemarks = "AR" 
        };
        Assert.True(dto.IsApproved);
        Assert.Equal("RR", dto.RejectionReason);
        Assert.Equal("Low", dto.RiskCategory);
        Assert.Equal("AR", dto.AgentRemarks);
    }
}
