using Domain.Enums;
using Xunit;

namespace Domain.Tests.Enums;

public class DocumentStatusTests
{
    [Fact]
    public void DocumentStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal("Uploaded", DocumentStatus.Uploaded.ToString());
    }
}
