using Application.DTOs.Policy;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class PremiumCalculationService : IPremiumCalculationService
{
    public PremiumCalculationResultDto Calculate(
        InsurancePlan plan,
        decimal sumAssured,
        int tenureYears,
        string riskCategory)
    {
        // Step 1 — Pick the correct risk multiplier
        var multiplier = riskCategory.ToLower() switch
        {
            "low" => plan.LowRiskMultiplier,
            "medium" or "standard" => plan.MediumRiskMultiplier,
            "high" => plan.HighRiskMultiplier,
            _ => throw new InvalidOperationException(
                "Invalid risk category. Must be Low, Standard/Medium, or High.")
        };

        // Step 2 — Core premium formula
        // Annual Premium = (SumAssured / 1000) × BaseRatePer1000 × RiskMultiplier
        // Example: SumAssured=1000000, BaseRate=1.5, Multiplier=1.25
        //          = (1000000/1000) × 1.5 × 1.25 = 1000 × 1.875 = ₹1875/year
        var annualPremium = (sumAssured / 1000m) * plan.BaseRatePer1000 * multiplier;

        // Round to 2 decimal places
        annualPremium = Math.Round(annualPremium, 2);

        var monthlyPremium = Math.Round(annualPremium / 12, 2);
        var quarterlyPremium = Math.Round(annualPremium / 4, 2);
        var totalOverTenure = Math.Round(annualPremium * tenureYears, 2);

        // Step 3 — Agent commission
        var commissionAmount = Math.Round(
            annualPremium * plan.CommissionPercentage / 100, 2);

        return new PremiumCalculationResultDto
        {
            SumAssured = sumAssured,
            TenureYears = tenureYears,
            RiskCategory = riskCategory,
            BaseRatePer1000 = plan.BaseRatePer1000,
            RiskMultiplierApplied = multiplier,
            AnnualPremium = annualPremium,
            MonthlyPremium = monthlyPremium,
            QuarterlyPremium = quarterlyPremium,
            TotalPremiumOverTenure = totalOverTenure,
            CommissionAmount = commissionAmount,
            CommissionPercentage = plan.CommissionPercentage
        };
    }
}