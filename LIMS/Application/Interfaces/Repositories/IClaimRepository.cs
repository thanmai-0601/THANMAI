using Application.DTOs.Dashboard;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(int claimId);
    Task<Claim?> GetByIdWithDetailsAsync(int claimId);
    Task<List<Claim>> GetByCustomerIdAsync(int customerId);
    Task<List<Claim>> GetByOfficerIdAsync(int officerId);
    Task<List<Claim>> GetAllAsync(ClaimStatus? statusFilter = null);
    Task<Claim> CreateAsync(Claim claim);
    Task<Claim> UpdateAsync(Claim claim);
    Task UpdateRangeAsync(IEnumerable<Claim> claims);
    Task<string> GenerateClaimNumberAsync();
    Task<int> GetActiveCountByOfficerAsync(int officerId); // for assignment logic
    Task AddDocumentAsync(ClaimDocument document);
    Task<DateTime?> GetLastAssignmentDateAsync(int officerId);
    Task<List<ClaimStatusCountDto>> GetClaimStatusCountsAsync();
    Task<List<ClaimStatusCountDto>> GetClaimStatusCountsByOfficerAsync(int officerId);
    Task<List<ClaimStatusCountDto>> GetClaimStatusCountsByCustomerAsync(int customerId);

    Task<decimal> GetTotalSettledAmountAsync();
    Task<decimal> GetTotalSettledByOfficerAsync(int officerId);
    Task<decimal> GetThisMonthSettledByOfficerAsync(int officerId);
    Task<decimal> GetTotalSettledByCustomerAsync(int customerId);

}