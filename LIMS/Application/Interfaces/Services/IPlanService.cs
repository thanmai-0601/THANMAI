using Application.DTOs.Policy;

namespace Application.Interfaces.Services;

public interface IPlanService
{
    // Admin operations
    Task<PlanResponseDto> CreatePlanAsync(CreatePlanDto dto);
    Task<PlanResponseDto> UpdatePlanAsync(int planId, UpdatePlanDto dto);
    Task DeletePlanAsync(int planId);

    // Both Admin and Customer
    Task<List<PlanResponseDto>> GetAllPlansAsync(bool includeInactive = false);
    Task<PlanResponseDto> GetPlanByIdAsync(int planId);
}