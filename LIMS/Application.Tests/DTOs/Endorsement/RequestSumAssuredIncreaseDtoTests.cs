using Application.DTOs.Endorsement;
using Xunit;

namespace Application.Tests.DTOs.Endorsement;

public class RequestSumAssuredIncreaseDtoTests
{
    [Fact]
    public void RequestSumAssuredIncreaseDto_PropertyAccessors_Work()
    {
        var dto = new RequestSumAssuredIncreaseDto { PolicyId = 1, NewSumAssured = 1000 };
        Assert.Equal(1, dto.PolicyId);
        Assert.Equal(1000, dto.NewSumAssured);
    }
}
