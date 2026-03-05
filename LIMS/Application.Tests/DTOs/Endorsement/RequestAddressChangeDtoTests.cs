using Application.DTOs.Endorsement;
using Xunit;

namespace Application.Tests.DTOs.Endorsement;

public class RequestAddressChangeDtoTests
{
    [Fact]
    public void RequestAddressChangeDto_PropertyAccessors_Work()
    {
        var dto = new RequestAddressChangeDto { PolicyId = 1, NewAddress = "A" };
        Assert.Equal(1, dto.PolicyId);
        Assert.Equal("A", dto.NewAddress);
    }
}
