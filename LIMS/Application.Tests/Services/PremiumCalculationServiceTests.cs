using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities;
using Xunit;

namespace Application.Tests.Services;

public class PremiumCalculationServiceTests
{
    private readonly PremiumCalculationService _service;

    public PremiumCalculationServiceTests()
    {
        _service = new PremiumCalculationService();
    }

    [Fact]
    public void Calculate_ShouldReturnCorrectAnnualPremium_ForLowRisk()
    {
        // Arrange
        var plan = new InsurancePlan
        {
            BaseRatePer1000 = 1.5m,
            LowRiskMultiplier = 1.25m,
            CommissionPercentage = 10
        };
        var sumAssured = 1000000m;
        var tenure = 20;

        // Act
        var result = _service.Calculate(plan, sumAssured, tenure, "Low");

        // Assert
        // (1000000 / 1000) * 1.5 * 1.25 = 1000 * 1.875 = 1875
        Assert.Equal(1875m, result.AnnualPremium);
        Assert.Equal(187.5m, result.CommissionAmount);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenRiskCategoryIsInvalid()
    {
        // Arrange
        var plan = new InsurancePlan();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _service.Calculate(plan, 100000, 10, "Invalid"));
    }
}
