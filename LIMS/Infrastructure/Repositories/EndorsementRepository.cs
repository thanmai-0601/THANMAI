using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EndorsementRepository : IEndorsementRepository
{
    private readonly AppDbContext _context;

    public EndorsementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PolicyEndorsement?> GetByIdAsync(int endorsementId)
        => await _context.PolicyEndorsements.FindAsync(endorsementId);

    public async Task<PolicyEndorsement?> GetByIdWithDetailsAsync(int endorsementId)
        => await _context.PolicyEndorsements
            .Include(e => e.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(e => e.RequestedByCustomer)
            .Include(e => e.ReviewedByAgent)
            .FirstOrDefaultAsync(e => e.Id == endorsementId);

    public async Task<List<PolicyEndorsement>> GetByPolicyIdAsync(int policyId)
        => await _context.PolicyEndorsements
            .Include(e => e.RequestedByCustomer)
            .Include(e => e.ReviewedByAgent)
            .Where(e => e.PolicyId == policyId)
            .OrderByDescending(e => e.RequestedAt)
            .ToListAsync();

    public async Task<List<PolicyEndorsement>> GetByCustomerIdAsync(int customerId)
        => await _context.PolicyEndorsements
            .Include(e => e.Policy)
            .Where(e => e.RequestedByCustomerId == customerId)
            .OrderByDescending(e => e.RequestedAt)
            .ToListAsync();

    // Agent sees only endorsements for their assigned policies
   

    public async Task<List<PolicyEndorsement>> GetAllAsync(
        EndorsementStatus? status = null)
    {
        var query = _context.PolicyEndorsements
            .Include(e => e.Policy)
            .Include(e => e.RequestedByCustomer)
            .Include(e => e.ReviewedByAgent)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        return await query
            .OrderByDescending(e => e.RequestedAt)
            .ToListAsync();
    }

    // Block multiple open endorsements on same policy
    public async Task<bool> HasPendingEndorsementAsync(int policyId)
        => await _context.PolicyEndorsements
            .AnyAsync(e =>
                e.PolicyId == policyId &&
                (e.Status == EndorsementStatus.Requested ||
                 e.Status == EndorsementStatus.UnderReview));

    public async Task<PolicyEndorsement> CreateAsync(PolicyEndorsement endorsement)
    {
        _context.PolicyEndorsements.Add(endorsement);
        await _context.SaveChangesAsync();
        return endorsement;
    }

    public async Task<PolicyEndorsement> UpdateAsync(PolicyEndorsement endorsement)
    {
        endorsement.UpdatedAt = DateTime.UtcNow;
        _context.PolicyEndorsements.Update(endorsement);
        await _context.SaveChangesAsync();
        return endorsement;
    }
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.PolicyEndorsements.CountAsync();
    }
    public async Task<int> GetPendingCountAsync()
    {
        return await _context.PolicyEndorsements
            .CountAsync(e =>
                e.Status == EndorsementStatus.Requested ||
                e.Status == EndorsementStatus.UnderReview);
    }


    public async Task<int> GetPendingByCustomerAsync(int customerId)
    => await _context.PolicyEndorsements
        .CountAsync(e =>
            e.RequestedByCustomerId == customerId &&
            (e.Status == EndorsementStatus.Requested ||
             e.Status == EndorsementStatus.UnderReview));

    public async Task<List<PolicyEndorsement>> GetPendingByAgentAsync(int agentId)
        => await _context.PolicyEndorsements
            .Include(e => e.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(e => e.RequestedByCustomer)
            .Where(e =>
                e.Policy.AgentId == agentId &&
                (e.Status == EndorsementStatus.Requested ||
                 e.Status == EndorsementStatus.UnderReview))
            .OrderByDescending(e => e.RequestedAt)
            .ToListAsync();
}