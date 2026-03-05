using Application.DTOs.Dashboard;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<List<User>> GetAllAsync(UserRole? roleFilter = null);
    Task<User> UpdateAsync(User user);
    Task<bool> EmailExistsExcludingUserAsync(string email, int excludeUserId); // ← for edit validation
    Task<List<User>> GetActiveAgentsAsync();
    Task<List<User>> GetActiveClaimsOfficersAsync();
    Task<List<UserRoleCountDto>> GetUserRoleCountsAsync();
}