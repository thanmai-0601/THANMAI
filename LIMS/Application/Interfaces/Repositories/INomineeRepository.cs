using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface INomineeRepository
{
    Task<List<Nominee>> GetByPolicyIdAsync(int policyId);
    Task AddRangeAsync(List<Nominee> nominees);
    Task DeleteByPolicyIdAsync(int policyId);  // clear and re-add on resubmit
}