using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                !u.IsDeleted);  // deleted users cannot login

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                !u.IsDeleted);

    // Used during edit — ignore the current user's own email
    public async Task<bool> EmailExistsExcludingUserAsync(string email, int excludeUserId)
        => await _context.Users
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                u.Id != excludeUserId &&
                !u.IsDeleted);

    // Returns all non-deleted users, optionally filtered by role
    public async Task<List<User>> GetAllAsync(UserRole? roleFilter = null)
    {
        var query = _context.Users
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        if (roleFilter.HasValue)
            query = query.Where(u => u.Role == roleFilter.Value);

        return await query
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<List<User>> GetActiveAgentsAsync()
    {
        return await _context.Users
        .Where(u => u.Role == UserRole.Agent &&
                    u.IsActive &&
                    !u.IsDeleted)
        .ToListAsync();
    }

    public async Task<List<User>> GetActiveClaimsOfficersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.ClaimsOfficer && u.IsActive)
            .ToListAsync();
    }
    public async Task<List<UserRoleCountDto>> GetUserRoleCountsAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .GroupBy(u => u.Role)
            .Select(g => new UserRoleCountDto
            {
                Role = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }
}