using Application.DTOs.Policy;
using Xunit;

namespace Application.Tests.DTOs.Policy;

public class RequestPolicyDtoTests
{
    [Fact]
    public void RequestPolicyDto_PropertyAccessors_Work()
    {
        var dto = new RequestPolicyDto 
        { 
            InsurancePlanId = 1, 
            TenureYears = 10,
            SumAssured = 500000,
            AnnualIncome = 50000,
            Occupation = "Engineer",
            Address = "123 Main St",
            CustomerAge = 30
        };
        Assert.Equal(1, dto.InsurancePlanId);
        Assert.Equal(10, dto.TenureYears);
        Assert.Equal(500000, dto.SumAssured);
        Assert.Equal("123 Main St", dto.Address);
    }
}
