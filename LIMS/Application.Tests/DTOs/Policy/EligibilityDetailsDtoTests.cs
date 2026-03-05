using Application.DTOs.Policy;
using Xunit;

namespace Application.Tests.DTOs.Policy;

public class EligibilityDetailsDtoTests
{
    [Fact]
    public void EligibilityDetailsDto_PropertyAccessors_Work()
    {
        var dto = new EligibilityDetailsDto 
        { 
            CustomerAge = 30, 
            AnnualIncome = 50000, 
            Occupation = "O", 
            RiskCategory = "Low" 
        };
        Assert.Equal(30, dto.CustomerAge);
        Assert.Equal(50000, dto.AnnualIncome);
    }
}
