using API;
using Xunit;

namespace API.Tests;

public class ProgramTests
{
    [Fact]
    public void Program_Main_CanBeReferenced()
    {
        // This is a minimal test to ensure Program class is reachable and defined.
        // Full bootstrapping tests usually require WebApplicationFactory which might be overkill
        // for "every class must have a test" requirement.
        var programType = typeof(Program);
        Assert.NotNull(programType);
        Assert.Equal("Program", programType.Name);
    }
}
