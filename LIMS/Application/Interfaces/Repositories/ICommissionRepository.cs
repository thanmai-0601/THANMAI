using Application.DTOs.Dashboard;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ICommissionRepository
{
    Task<Commission> CreateAsync(Commission commission);
    Task<List<Commission>> GetByAgentIdAsync(int agentId);
    Task<decimal> GetTotalCommissionAsync();
    Task<decimal> GetTotalPendingCommissionAsync();
    Task<decimal> GetTotalByAgentAsync(int agentId);
    Task<decimal> GetPendingTotalByAgentAsync(int agentId);
    Task<decimal> GetThisMonthByAgentAsync(int agentId);
    Task<decimal> GetLastMonthByAgentAsync(int agentId);
    Task<List<PolicyCommissionDto>> GetRecentByAgentAsync(int agentId);
    Task<Commission?> GetByPolicyIdAsync(int policyId);
    Task UpdateAsync(Commission commission);
}