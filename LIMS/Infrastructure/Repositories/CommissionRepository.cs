using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommissionRepository : ICommissionRepository
{
    private readonly AppDbContext _context;

    public CommissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Commission> CreateAsync(Commission commission)
    {
        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync();
        return commission;
    }

    public async Task<Commission?> GetByPolicyIdAsync(int policyId)
    {
        return await _context.Commissions
            .Include(c => c.Agent)
            .FirstOrDefaultAsync(c => c.PolicyId == policyId);
    }

    public async Task UpdateAsync(Commission commission)
    {
        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Commission>> GetByAgentIdAsync(int agentId)
        => await _context.Commissions
            .Include(c => c.Policy)
            .Where(c => c.AgentId == agentId && c.Status == Domain.Enums.CommissionStatus.Earned)
            .OrderByDescending(c => c.EarnedOn)
            .ToListAsync();
    public async Task<decimal> GetTotalCommissionAsync()
    {
        return await _context.Commissions
            .Where(c => c.Status != Domain.Enums.CommissionStatus.Pending)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<decimal> GetTotalPendingCommissionAsync()
    {
        return await _context.Commissions
            .Where(c => c.Status == Domain.Enums.CommissionStatus.Pending)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<decimal> GetTotalByAgentAsync(int agentId)
    {
        return await _context.Commissions
            .Where(c => c.AgentId == agentId && c.Status != Domain.Enums.CommissionStatus.Pending)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<decimal> GetPendingTotalByAgentAsync(int agentId)
    {
        return await _context.Commissions
            .Where(c => c.AgentId == agentId && c.Status == Domain.Enums.CommissionStatus.Pending)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<decimal> GetThisMonthByAgentAsync(int agentId)
    {
        var startOfMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        return await _context.Commissions
            .Where(c =>
                c.AgentId == agentId &&
                c.Status != Domain.Enums.CommissionStatus.Pending &&
                c.EarnedOn >= startOfMonth)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<decimal> GetLastMonthByAgentAsync(int agentId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        return await _context.Commissions
            .Where(c =>
                c.AgentId == agentId &&
                c.Status != Domain.Enums.CommissionStatus.Pending &&
                c.EarnedOn >= startOfLastMonth &&
                c.EarnedOn < startOfMonth)
            .SumAsync(c => c.CommissionAmount);
    }
    public async Task<List<PolicyCommissionDto>>
    GetRecentByAgentAsync(int agentId)
    {
        return await _context.Commissions
            .Include(c => c.Policy)
                .ThenInclude(p => p.Customer)
            .Where(c => c.AgentId == agentId && c.Status == Domain.Enums.CommissionStatus.Earned)
            .OrderByDescending(c => c.EarnedOn)
            .Take(10)
            .Select(c => new PolicyCommissionDto
            {
                PolicyNumber = c.Policy.PolicyNumber,
                CustomerName = c.Policy.Customer.FullName,
                PremiumAmount = c.PremiumAmount,
                CommissionAmount = c.CommissionAmount,
                CommissionPercentage = c.CommissionPercentage,
                Status = c.Status.ToString(),
                EarnedOn = c.EarnedOn
            })
            .ToListAsync();
    }
}