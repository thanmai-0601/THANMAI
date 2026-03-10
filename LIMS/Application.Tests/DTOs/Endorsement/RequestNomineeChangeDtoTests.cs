using Application.DTOs.Endorsement;
using Xunit;
using System.Collections.Generic;
using Application.DTOs.Policy;

namespace Application.Tests.DTOs.Endorsement;

public class RequestNomineeChangeDtoTests
{
    [Fact]
    public void RequestNomineeChangeDto_PropertyAccessors_Work()
    {
        var dto = new RequestNomineeChangeDto { PolicyId = 1, NewNominee = new AddNomineeDto() };
        Assert.Equal(1, dto.PolicyId);
        Assert.NotNull(dto.NewNominee);
    }
}
