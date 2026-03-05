using Application.DTOs.Claim;
using Xunit;
using System;

namespace Application.Tests.DTOs.Claim;

public class RaiseClaimDtoTests
{
    [Fact]
    public void RaiseClaimDto_PropertyAccessors_Work()
    {
        var dto = new RaiseClaimDto 
        { 
            PolicyNumber = "PN", 
            CauseOfDeath = "C", 
            DateOfDeath = DateTime.UtcNow,
            NomineeName = "N",
            NomineeRelationship = "R",
            BankAccountName = "BN",
            BankAccountNumber = "123",
            BankIfscCode = "IFSC"
        };
        Assert.Equal("PN", dto.PolicyNumber);
        Assert.Equal("C", dto.CauseOfDeath);
        Assert.Equal("N", dto.NomineeName);
    }
}
