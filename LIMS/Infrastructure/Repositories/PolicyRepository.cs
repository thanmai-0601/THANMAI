using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _context;

    public PolicyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Policy?> GetByIdAsync(int policyId)
        => await _context.Policies.FindAsync(policyId);

    public async Task<Policy?> GetByIdWithDetailsAsync(int policyId)
        => await _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Agent)
            .Include(p => p.InsurancePlan)
            .Include(p => p.Nominees)
            .Include(p => p.Documents)
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == policyId);

    public async Task<Policy?> GetByPolicyNumberWithDetailsAsync(string policyNumber)
        => await _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Agent)
            .Include(p => p.InsurancePlan)
            .Include(p => p.Nominees)
            .Include(p => p.Documents)
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);

    public async Task<List<Policy>> GetByCustomerIdAsync(int customerId)
        => await _context.Policies
            .Include(p => p.InsurancePlan)
            .Include(p => p.Agent)
            .Include(p => p.Claims)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<Policy>> GetByAgentIdAsync(int agentId)
        => await _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.InsurancePlan)
            .Include(p => p.Claims)
            .Where(p => p.AgentId == agentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<Policy>> GetAllAsync(PolicyStatus? statusFilter = null)
    {
        var query = _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Agent)
            .Include(p => p.InsurancePlan)
            .Include(p => p.Claims)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(p => p.Status == statusFilter.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Policy> CreateAsync(Policy policy)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy)
    {
        policy.UpdatedAt = DateTime.UtcNow;
        _context.Policies.Update(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task UpdateRangeAsync(IEnumerable<Policy> policies)
    {
        var now = DateTime.UtcNow;
        foreach (var p in policies)
        {
            p.UpdatedAt = now;
        }
        _context.Policies.UpdateRange(policies);
        await _context.SaveChangesAsync();
    }

    // Generates sequential policy number: POL-20240001, POL-20240002...
    public async Task<string> GeneratePolicyNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"POL-{year}";

        var lastPolicy = await _context.Policies
            .Where(p => p.PolicyNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PolicyNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastPolicy != null)
        {
            // Extract the number part from "POL-20240042" → 42 → next is 43
            var lastNumber = int.Parse(lastPolicy.PolicyNumber.Split('-')[1][4..]);
            nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D4}"; // D4 = zero-padded to 4 digits
    }

    public async Task<int> GetActiveCountByAgentAsync(int agentId)
        => await _context.Policies
            .CountAsync(p =>
                p.AgentId == agentId &&
                p.Status != PolicyStatus.Rejected &&
                p.Status != PolicyStatus.Cancelled &&
                p.Status != PolicyStatus.Lapsed);

    public async Task<DateTime?> GetLastAssignmentDateAsync(int agentId)
    {
        return await _context.Policies
            .Where(p => p.AgentId == agentId)
            .OrderByDescending(p => p.AgentAssignedAt)
            .Select(p => p.AgentAssignedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddDocumentAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task AddNomineeAsync(Nominee nominee)
    {
        _context.Nominees.Add(nominee);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveNomineesAsync(int policyId)
    {
        var nominees = await _context.Nominees.Where(n => n.PolicyId == policyId).ToListAsync();
        _context.Nominees.RemoveRange(nominees);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsAsync()
    {
        return await _context.Policies
            .GroupBy(p => p.Status)
            .Select(g => new PolicyStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<int> GetActiveWithSettledClaimCountAsync()
    {
        return await _context.Policies
            .CountAsync(p => p.Status == PolicyStatus.Active && p.Claims.Any(c => c.Status == ClaimStatus.Settled));
    }

    public async Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsByAgentAsync(int agentId)
    {
        return await _context.Policies
            .Where(p => p.AgentId == agentId)
            .GroupBy(p => p.Status)
            .Select(g => new PolicyStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<PolicyStatusCountDto>> GetPolicyStatusCountsByCustomerAsync(int customerId)
    {
        return await _context.Policies
            .Where(p => p.CustomerId == customerId)
            .GroupBy(p => p.Status)
            .Select(g => new PolicyStatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }


    public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.Agent && u.IsActive)
            .Select(agent => new AgentPerformanceDto
            {
                AgentId = agent.Id,
                AgentName = agent.FullName,
                Email = agent.Email,
                TotalPoliciesAssigned = agent.AgentPolicies.Count,
                ActivePolicies = agent.AgentPolicies
                    .Count(p => p.Status == PolicyStatus.Active && !p.Claims.Any(c => c.Status == ClaimStatus.Settled)),
                ApprovedPolicies = agent.AgentPolicies
                    .Count(p => p.Status == PolicyStatus.Active && !p.Claims.Any(c => c.Status == ClaimStatus.Settled)),
                RejectedPolicies = agent.AgentPolicies
                    .Count(p => p.Status == PolicyStatus.Rejected),
                TotalCommissionEarned = _context.Commissions
                    .Where(c => c.AgentId == agent.Id)
                    .Sum(c => c.CommissionAmount),
                ConversionRate = agent.AgentPolicies.Count == 0 ? 0 :
                    Math.Round(
                        (decimal)agent.AgentPolicies
                            .Count(p => p.Status == PolicyStatus.Active && !p.Claims.Any(c => c.Status == ClaimStatus.Settled))
                        / agent.AgentPolicies.Count * 100, 1)
            })
            .ToListAsync();
    }

    public async Task<List<PlanDistributionDto>> GetPlanDistributionAsync()
    {
        return await _context.InsurancePlans
            .Select(plan => new PlanDistributionDto
            {
                PlanId = plan.Id,
                PlanName = plan.PlanName,
                TotalPolicies = plan.Policies.Count,
                ActivePolicies = plan.Policies
                    .Count(p => p.Status == PolicyStatus.Active && !p.Claims.Any(c => c.Status == ClaimStatus.Settled)),
                TotalSumAssured = plan.Policies
                    .Where(p => p.Status == PolicyStatus.Active && !p.Claims.Any(c => c.Status == ClaimStatus.Settled))
                    .Sum(p => p.SumAssured)
            })
            .ToListAsync();
    }

    public async Task<List<RecentActivityDto>>
    GetRecentActivityByCustomerAsync(int customerId)
    {
        return await _context.Policies
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(8)
            .Select(p => new RecentActivityDto
            {
                Type = "Policy",
                Description = $"Policy {p.PolicyNumber}",
                Status = p.Status.ToString(),
                Date = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync();
    }
}