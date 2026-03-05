using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InsurancePlan>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.InsurancePlans.AsQueryable();

        // Customers only see active plans; Admin sees all
        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.PlanName)
            .ToListAsync();
    }

    public async Task<InsurancePlan?> GetByIdAsync(int id)
        => await _context.InsurancePlans
            .Include(p => p.Policies)  // needed for TotalPoliciesCount
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> PlanNameExistsAsync(string planName)
        => await _context.InsurancePlans
            .AnyAsync(p => p.PlanName.ToLower() == planName.ToLower());

    public async Task<bool> PlanNameExistsExcludingAsync(string planName, int excludeId)
        => await _context.InsurancePlans
            .AnyAsync(p =>
                p.PlanName.ToLower() == planName.ToLower() &&
                p.Id != excludeId);

    public async Task<InsurancePlan> CreateAsync(InsurancePlan plan)
    {
        _context.InsurancePlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<InsurancePlan> UpdateAsync(InsurancePlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _context.InsurancePlans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task DeleteAsync(InsurancePlan plan)
    {
        _context.InsurancePlans.Remove(plan);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasActivePoliciesAsync(int planId)
        => await _context.Policies
            .AnyAsync(p => p.InsurancePlanId == planId);
    // once Policy entity exists this will work automatically
}