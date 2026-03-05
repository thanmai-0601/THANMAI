using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IEndorsementRepository
{
    Task<PolicyEndorsement?> GetByIdAsync(int endorsementId);
    Task<PolicyEndorsement?> GetByIdWithDetailsAsync(int endorsementId);
    Task<List<PolicyEndorsement>> GetByPolicyIdAsync(int policyId);
    Task<List<PolicyEndorsement>> GetPendingByAgentAsync(int agentId);
    Task<List<PolicyEndorsement>> GetByCustomerIdAsync(int customerId);
    Task<List<PolicyEndorsement>> GetAllAsync(EndorsementStatus? status = null);
    Task<bool> HasPendingEndorsementAsync(int policyId);
    Task<PolicyEndorsement> CreateAsync(PolicyEndorsement endorsement);
    Task<PolicyEndorsement> UpdateAsync(PolicyEndorsement endorsement);
    Task<int> GetTotalCountAsync();
    Task<int> GetPendingCountAsync();
    Task<int> GetPendingByCustomerAsync(int customerId);
}