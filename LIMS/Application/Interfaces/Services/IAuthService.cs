using Application.DTOs.Auth;
using Domain.Enums;


namespace Application.Interfaces.Services;

public interface IAuthService
{
    // Public
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    // Admin — Staff creation
    Task<StaffResponseDto> CreateStaffAsync(CreateStaffDto dto,UserRole role);

    // Admin — User management
    Task<List<UserListDto>> GetAllUsersAsync(UserRole? roleFilter = null);
    Task<UserListDto> GetUserByIdAsync(int userId);
    Task<UserListDto> UpdateStaffAsync(int userId, UpdateStaffDto dto);  // edit details + role
    Task DeleteUserAsync(int userId);                                     // soft delete
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task ToggleUserStatusAsync(int userId);

}