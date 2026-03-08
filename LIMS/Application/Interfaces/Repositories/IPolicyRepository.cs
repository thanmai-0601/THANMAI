using Application.DTOs.Dashboard;
using Domain.Entities;
using Domain.Enums;
namespace Application.Interfaces.Repositories;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(int policyId);
    Task<Policy?> GetByIdWithDetailsAsync(int policyId);   // includes Customer, Agent, Plan
    Task<Policy?> GetByPolicyNumberWithDetailsAsync(string policyNumber); // Includes Customer, Agent, Plan
    Task<List<Policy>> GetByCustomerIdAsync(int customerId);
    Task<List<Policy>> GetByAgentIdAsync(int agentId);
    Task<List<Policy>> GetAllAsync(PolicyStatus? statusFilter = null);
    Task<Policy> CreateAsync(Policy policy);
    Task<Policy> UpdateAsync(Policy policy);
    Task UpdateRangeAsync(IEnumerable<Policy> policies);
    Task<string> GeneratePolicyNumberAsync();              // generates "POL-20240001"
    Task<int> GetActiveCountByAgentAsync(int agentId);    // for assignment logic
    Task<DateTime?> GetLastAssignmentDateAsync(int agentId);
    Task AddDocumentAsync(Document document);
    Task AddNomineeAsync(Nominee nominee);
    Task RemoveNomineesAsync(int policyId);
    Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsAsync();
    Task<int> GetActiveWithSettledClaimCountAsync();
    Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsByAgentAsync(int agentId);
    Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsByCustomerAsync(int customerId);

    Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
    Task<List<PlanDistributionDto>> GetPlanDistributionAsync();
    Task<List<RecentActivityDto>> GetRecentActivityByCustomerAsync(int customerId);
}