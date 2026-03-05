using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly AppDbContext _context;

    public ClaimRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Claim?> GetByIdAsync(int claimId)
        => await _context.Claims.FindAsync(claimId);

    public async Task<Claim?> GetByIdWithDetailsAsync(int claimId)
        => await _context.Claims
            .Include(c => c.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(c => c.Policy)
                .ThenInclude(p => p.Nominees)
            .Include(c => c.Policy)
                .ThenInclude(p => p.Invoices)
            .Include(c => c.Customer)
            .Include(c => c.ClaimsOfficer)
            .Include(c => c.ClaimDocuments)
            .FirstOrDefaultAsync(c => c.Id == claimId);


    public async Task<List<Claim>> GetByCustomerIdAsync(int customerId)
        => await _context.Claims
            .Include(c => c.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(c => c.ClaimsOfficer)
            .Include(c => c.ClaimDocuments)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync();

    public async Task<List<Claim>> GetByOfficerIdAsync(int officerId)
        => await _context.Claims
            .Include(c => c.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(c => c.Customer)
            .Include(c => c.ClaimDocuments)
            .Where(c => c.ClaimsOfficerId == officerId)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync();

    public async Task<List<Claim>> GetAllAsync(ClaimStatus? statusFilter = null)
    {
        var query = _context.Claims
            .Include(c => c.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(c => c.Customer)
            .Include(c => c.ClaimsOfficer)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(c => c.Status == statusFilter.Value);

        return await query
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Claim> CreateAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task<Claim> UpdateAsync(Claim claim)
    {
        claim.UpdatedAt = DateTime.UtcNow;
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task UpdateRangeAsync(IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        foreach (var c in claims)
        {
            c.UpdatedAt = now;
        }
        _context.Claims.UpdateRange(claims);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateClaimNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CLM-{year}";

        var last = await _context.Claims
            .Where(c => c.ClaimNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ClaimNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (last != null)
        {
            var lastNum = int.Parse(last.ClaimNumber.Split('-')[1][4..]);
            nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    // Count claims that are not yet closed (for assignment load balancing)
    public async Task<int> GetActiveCountByOfficerAsync(int officerId)
        => await _context.Claims
            .CountAsync(c =>
                c.ClaimsOfficerId == officerId &&
                c.Status != ClaimStatus.Settled &&
                c.Status != ClaimStatus.Rejected);

    public async Task AddDocumentAsync(ClaimDocument document)
    {
        _context.ClaimDocuments.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task<DateTime?> GetLastAssignmentDateAsync(int officerId)
    {
        return await _context.Claims
            .Where(c => c.ClaimsOfficerId == officerId)
            .OrderByDescending(c => c.AssignedAt)
            .Select(c => c.AssignedAt)
            .FirstOrDefaultAsync();
    }
    public async Task<List<ClaimStatusCountDto>> GetClaimStatusCountsAsync()
    {
        return await _context.Claims
            .GroupBy(c => c.Status)
            .Select(g => new ClaimStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<ClaimStatusCountDto>> GetClaimStatusCountsByOfficerAsync(int officerId)
    {
        return await _context.Claims
            .Where(c => c.ClaimsOfficerId == officerId)
            .GroupBy(c => c.Status)
            .Select(g => new ClaimStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<ClaimStatusCountDto>> GetClaimStatusCountsByCustomerAsync(int customerId)
    {
        return await _context.Claims
            .Where(c => c.CustomerId == customerId)
            .GroupBy(c => c.Status)
            .Select(g => new ClaimStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }
   
    public async Task<decimal> GetTotalSettledAmountAsync()
    {
        return await _context.Claims
            .Where(c => c.Status == ClaimStatus.Settled)
            .SumAsync(c => c.SettledAmount ?? 0);
    }
    public async Task<decimal> GetTotalSettledByOfficerAsync(int officerId)
    {
        return await _context.Claims
            .Where(c => c.ClaimsOfficerId == officerId &&
                        c.Status == ClaimStatus.Settled)
            .SumAsync(c => c.SettledAmount ?? 0);
    }
    public async Task<decimal>
    GetThisMonthSettledByOfficerAsync(int officerId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year,
                                        DateTime.UtcNow.Month, 1);

        return await _context.Claims
            .Where(c =>
                c.ClaimsOfficerId == officerId &&
                c.Status == ClaimStatus.Settled &&
                c.SettledAt >= startOfMonth)
            .SumAsync(c => c.SettledAmount ?? 0);
    }
    public async Task<decimal> GetTotalSettledByCustomerAsync(int customerId)
    {
        return await _context.Claims
            .Where(c => c.CustomerId == customerId &&
                        c.Status == ClaimStatus.Settled)
            .SumAsync(c => c.SettledAmount ?? 0);
    }

}