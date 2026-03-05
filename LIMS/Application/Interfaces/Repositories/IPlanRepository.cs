using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IPlanRepository
{
    Task<List<InsurancePlan>> GetAllAsync(bool includeInactive = false);
    Task<InsurancePlan?> GetByIdAsync(int id);
    Task<bool> PlanNameExistsAsync(string planName);
    Task<bool> PlanNameExistsExcludingAsync(string planName, int excludeId);
    Task<InsurancePlan> CreateAsync(InsurancePlan plan);
    Task<InsurancePlan> UpdateAsync(InsurancePlan plan);
    Task DeleteAsync(InsurancePlan plan);
    Task<bool> HasActivePoliciesAsync(int planId);  // safety check before delete
}