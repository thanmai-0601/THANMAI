using Application.DTOs.Policy;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

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

        // Step 2 — Core premium formula (common to all plan types)
        // Annual Premium = (SumAssured / 1000) × BaseRatePer1000 × RiskMultiplier
        var annualPremium = (sumAssured / 1000m) * plan.BaseRatePer1000 * multiplier;

        // Endowment plans have higher premiums due to savings component
        // The higher BaseRatePer1000 already accounts for this, so no extra multiplier needed

        // Round to 2 decimal places
        annualPremium = Math.Round(annualPremium, 2);

        var totalOverTenure = Math.Round(annualPremium * tenureYears, 2);

        // Step 3 — Agent commission
        var commissionAmount = Math.Round(
            annualPremium * plan.CommissionPercentage / 100, 2);

        // Step 4 — Endowment maturity benefit calculation
        decimal estimatedBonus = 0;
        decimal maturityBenefit = 0;

        if (plan.PlanType == PlanType.Endowment && plan.BonusRatePerYear > 0)
        {
            // Estimated Bonus = SumAssured × (BonusRate / 100) × TenureYears
            estimatedBonus = Math.Round(
                sumAssured * (plan.BonusRatePerYear / 100) * tenureYears, 2);

            // Maturity Benefit = SumAssured + Accumulated Bonus
            maturityBenefit = Math.Round(sumAssured + estimatedBonus, 2);
        }

        return new PremiumCalculationResultDto
        {
            PlanType = plan.PlanType,
            SumAssured = sumAssured,
            TenureYears = tenureYears,
            RiskCategory = riskCategory,
            BaseRatePer1000 = plan.BaseRatePer1000,
            RiskMultiplierApplied = multiplier,
            AnnualPremium = annualPremium,
            TotalPremiumOverTenure = totalOverTenure,
            CommissionAmount = commissionAmount,
            CommissionPercentage = plan.CommissionPercentage,
            EstimatedBonus = estimatedBonus,
            MaturityBenefit = maturityBenefit
        };
    }
}