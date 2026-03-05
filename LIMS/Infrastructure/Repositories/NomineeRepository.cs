using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NomineeRepository : INomineeRepository
{
    private readonly AppDbContext _context;

    public NomineeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Nominee>> GetByPolicyIdAsync(int policyId)
        => await _context.Nominees
            .Where(n => n.PolicyId == policyId)
            .ToListAsync();

    public async Task AddRangeAsync(List<Nominee> nominees)
    {
        _context.Nominees.AddRange(nominees);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByPolicyIdAsync(int policyId)
    {
        var nominees = await _context.Nominees
            .Where(n => n.PolicyId == policyId)
            .ToListAsync();

        _context.Nominees.RemoveRange(nominees);
        await _context.SaveChangesAsync();
    }
}