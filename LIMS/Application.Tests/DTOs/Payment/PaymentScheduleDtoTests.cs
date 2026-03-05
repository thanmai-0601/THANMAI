using Application.DTOs.Payment;
using Xunit;
using System.Collections.Generic;

namespace Application.Tests.DTOs.Payment;

public class PaymentScheduleDtoTests
{
    [Fact]
    public void PaymentScheduleDto_PropertyAccessors_Work()
    {
        var dto = new PaymentScheduleDto 
        { 
            PolicyNumber = "PN", 
            Frequency = "Monthly", 
            InstallmentAmount = 1000, 
            TotalInstallments = 12, 
            TotalPayable = 12000,
            Invoices = new List<InvoiceResponseDto>()
        };
        Assert.Equal("PN", dto.PolicyNumber);
        Assert.Equal(1000, dto.InstallmentAmount);
        Assert.NotNull(dto.Invoices);
    }
}
