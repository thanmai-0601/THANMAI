using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;


namespace Application.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepo;

    public PlanService(IPlanRepository planRepo)
    {
        _planRepo = planRepo;
    }

    public async Task<List<PlanResponseDto>> GetAllPlansAsync(bool includeInactive = false)
    {
        var plans = await _planRepo.GetAllAsync(includeInactive);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<PlanResponseDto> GetPlanByIdAsync(int planId)
    {
        var plan = await _planRepo.GetByIdAsync(planId)
            ?? throw new KeyNotFoundException($"Plan with ID {planId} not found.");

        return MapToDto(plan);
    }

    public async Task<PlanResponseDto> CreatePlanAsync(CreatePlanDto dto)
    {
        // Validate sum assured range
        if (dto.MinSumAssured >= dto.MaxSumAssured)
            throw new InvalidOperationException(
                "Minimum sum assured must be less than maximum sum assured.");

        // Validate age range
        if (dto.MinEntryAge >= dto.MaxEntryAge)
            throw new InvalidOperationException(
                "Minimum entry age must be less than maximum entry age.");

        // Validate risk multipliers are in ascending order
        if (!(dto.LowRiskMultiplier <= dto.MediumRiskMultiplier &&
              dto.MediumRiskMultiplier <= dto.HighRiskMultiplier))
            throw new InvalidOperationException(
                "Risk multipliers must be in order: Low ≤ Medium ≤ High.");

        if (await _planRepo.PlanNameExistsAsync(dto.PlanName))
            throw new InvalidOperationException(
                $"A plan named '{dto.PlanName}' already exists.");

        var plan = new InsurancePlan
        {
            PlanName = dto.PlanName.Trim(),
            Description = dto.Description.Trim(),
            MinSumAssured = dto.MinSumAssured,
            MaxSumAssured = dto.MaxSumAssured,
            TenureOptions = string.Join(",", dto.TenureOptions.OrderBy(t => t)),
            AvailableRiders = dto.AvailableRiders,
            MinEntryAge = dto.MinEntryAge,
            MaxEntryAge = dto.MaxEntryAge,
            MinAnnualIncome = dto.MinAnnualIncome,
            BaseRatePer1000 = dto.BaseRatePer1000,
            LowRiskMultiplier = dto.LowRiskMultiplier,
            MediumRiskMultiplier = dto.MediumRiskMultiplier,
            HighRiskMultiplier = dto.HighRiskMultiplier,
            CommissionPercentage = dto.CommissionPercentage,
            IsActive = true
        };

        await _planRepo.CreateAsync(plan);
        return MapToDto(plan);
    }

    public async Task<PlanResponseDto> UpdatePlanAsync(int planId, UpdatePlanDto dto)
    {
        var plan = await _planRepo.GetByIdAsync(planId)
            ?? throw new KeyNotFoundException($"Plan with ID {planId} not found.");

        if (dto.MinSumAssured >= dto.MaxSumAssured)
            throw new InvalidOperationException(
                "Minimum sum assured must be less than maximum sum assured.");

        if (dto.MinEntryAge >= dto.MaxEntryAge)
            throw new InvalidOperationException(
                "Minimum entry age must be less than maximum entry age.");

        if (!(dto.LowRiskMultiplier <= dto.MediumRiskMultiplier &&
              dto.MediumRiskMultiplier <= dto.HighRiskMultiplier))
            throw new InvalidOperationException(
                "Risk multipliers must be in order: Low ≤ Medium ≤ High.");

        if (await _planRepo.PlanNameExistsExcludingAsync(dto.PlanName, planId))
            throw new InvalidOperationException(
                $"Another plan named '{dto.PlanName}' already exists.");

        plan.PlanName = dto.PlanName.Trim();
        plan.Description = dto.Description.Trim();
        plan.MinSumAssured = dto.MinSumAssured;
        plan.MaxSumAssured = dto.MaxSumAssured;
        plan.TenureOptions = string.Join(",", dto.TenureOptions.OrderBy(t => t));
        plan.AvailableRiders = dto.AvailableRiders;
        plan.MinEntryAge = dto.MinEntryAge;
        plan.MaxEntryAge = dto.MaxEntryAge;
        plan.MinAnnualIncome = dto.MinAnnualIncome;
        plan.BaseRatePer1000 = dto.BaseRatePer1000;
        plan.LowRiskMultiplier = dto.LowRiskMultiplier;
        plan.MediumRiskMultiplier = dto.MediumRiskMultiplier;
        plan.HighRiskMultiplier = dto.HighRiskMultiplier;
        plan.CommissionPercentage = dto.CommissionPercentage;
        plan.IsActive = dto.IsActive;

        await _planRepo.UpdateAsync(plan);
        return MapToDto(plan);
    }

    public async Task DeletePlanAsync(int planId)
    {
        var plan = await _planRepo.GetByIdAsync(planId)
            ?? throw new KeyNotFoundException($"Plan with ID {planId} not found.");

        // Safety check — don't delete a plan that has policies linked to it
        if (await _planRepo.HasActivePoliciesAsync(planId))
            throw new InvalidOperationException(
                "Cannot delete this plan — it has existing policies linked to it. " +
                "Deactivate the plan instead.");

        await _planRepo.DeleteAsync(plan);
    }

    // ── Private Mapper ─────────────────────────────────────────────────────

    private static PlanResponseDto MapToDto(InsurancePlan plan) => new()
    {
        PlanId = plan.Id,
        PlanName = plan.PlanName,
        Description = plan.Description,
        MinSumAssured = plan.MinSumAssured,
        MaxSumAssured = plan.MaxSumAssured,
        // Parse "10,15,20" back into [10, 15, 20]
        TenureOptions = plan.TenureOptions
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList(),
        AvailableRiders = plan.AvailableRiders,
        MinEntryAge = plan.MinEntryAge,
        MaxEntryAge = plan.MaxEntryAge,
        MinAnnualIncome = plan.MinAnnualIncome,
        BaseRatePer1000 = plan.BaseRatePer1000,
        LowRiskMultiplier = plan.LowRiskMultiplier,
        MediumRiskMultiplier = plan.MediumRiskMultiplier,
        HighRiskMultiplier = plan.HighRiskMultiplier,
        CommissionPercentage = plan.CommissionPercentage,
        IsActive = plan.IsActive,
        CreatedAt = plan.CreatedAt,
        TotalPoliciesCount = plan.Policies?.Count ?? 0
    };
}