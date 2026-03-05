using Application.DTOs.Policy;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IPremiumCalculationService
{
    PremiumCalculationResultDto Calculate(
        InsurancePlan plan,
        decimal sumAssured,
        int tenureYears,
        string riskCategory);
}