using Application.DTOs.Dashboard;
using Xunit;

namespace Application.Tests.DTOs.Dashboard;

public class RecentActivityDtoTests
{
    [Fact]
    public void RecentActivityDto_PropertyAccessors_Work()
    {
        var dto = new RecentActivityDto { Description = "D" };
        Assert.Equal("D", dto.Description);
    }
}
