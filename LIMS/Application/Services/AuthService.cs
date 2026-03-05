using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPolicyRepository _policyRepo;
    private readonly IClaimRepository _claimRepo;
    private readonly INotificationService _notificationService;

    public AuthService(
        IUserRepository userRepo, 
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        IPolicyRepository policyRepo,
        IClaimRepository claimRepo,
        INotificationService notificationService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _policyRepo = policyRepo;
        _claimRepo = claimRepo;
        _notificationService = notificationService;
    }

    // ── Public Registration — always Customer ──────────────────────────────

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.Customer,  // always Customer, no exceptions
            IsActive = true
        };

        await _userRepo.CreateAsync(user);
        return BuildAuthResponse(user);
    }

    // ── Login ──────────────────────────────────────────────────────────────

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Your account has been deactivated.");

        // ← This line must be present
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            MustChangePassword = false // Deprecated
        };
    }

    // ── Admin: Create Agent or ClaimsOfficer ───────────────────────────────

    public async Task<StaffResponseDto> CreateStaffAsync(CreateStaffDto dto,UserRole role)
    {
        if (role != UserRole.Agent && role != UserRole.ClaimsOfficer)
            throw new InvalidOperationException(
                "This endpoint can only create Agents or Claims Officers.");

        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = role,
            IsActive = true,
            MustChangePassword = false
        };

        await _userRepo.CreateAsync(user);

        return new StaffResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            TemporaryPassword = dto.Password,
            CreatedAt = user.CreatedAt
        };
    }

    // ── Admin: Get All Users ───────────────────────────────────────────────

    public async Task<List<UserListDto>> GetAllUsersAsync(UserRole? roleFilter=null)
    {
        var users = await _userRepo.GetAllAsync(roleFilter);
        return users.Select(MapToUserListDto).ToList();
    }

    // ── Admin: Get Single User ─────────────────────────────────────────────

    public async Task<UserListDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        return MapToUserListDto(user);
    }

    // ── Admin: Update Staff Details (Name, Email, Phone, Role) ────────────

    public async Task<UserListDto> UpdateStaffAsync(int userId, UpdateStaffDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        // Admin and Customer accounts cannot be edited through this endpoint
        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin details cannot be edited.");

        if (user.Role == UserRole.Customer)
            throw new InvalidOperationException(
                "Customer details cannot be edited by Admin. Customers manage their own profiles.");

        // Prevent changing role TO Admin or Customer via this endpoint
        if (dto.Role == UserRole.Admin || dto.Role == UserRole.Customer)
            throw new InvalidOperationException(
                "Role can only be changed between Agent and Claims Officer.");

        // Check new email isn't already taken by someone else
        if (await _userRepo.EmailExistsExcludingUserAsync(dto.Email, userId))
            throw new InvalidOperationException("This email is already used by another account.");

        // Apply updates
        user.FullName = dto.FullName;
        user.Email = dto.Email.ToLower().Trim();
        user.PhoneNumber = dto.PhoneNumber;
        user.Role = dto.Role;

        await _userRepo.UpdateAsync(user);
        return MapToUserListDto(user);
    }

    // ── Admin: Soft Delete User ────────────────────────────────────────────

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        // Protect Admin accounts from deletion
        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin account cannot be deleted.");

        // Soft delete — mark as deleted, don't remove from DB
        // This keeps all linked policies, claims, commissions intact
        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);
    }

    // ── Admin: Toggle User Status ──────────────────────────────────────────

    public async Task ToggleUserStatusAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin account status cannot be toggled.");

        user.IsActive = !user.IsActive;
        await _userRepo.UpdateAsync(user);

        // If deactivated, handle related policies and claims
        if (!user.IsActive)
        {
            if (user.Role == UserRole.Agent)
            {
                var policies = await _policyRepo.GetByAgentIdAsync(user.Id);
                var toUpdate = new List<Policy>();
                foreach (var policy in policies.Where(p => p.Status == PolicyStatus.Active || p.Status == PolicyStatus.Submitted || p.Status == PolicyStatus.UnderReview || p.Status == PolicyStatus.DocumentsSubmitted || p.Status == PolicyStatus.Approved))
                {
                    policy.RejectionReason = $"RECOVERY_AGENT_OFFLINE|{policy.Status}";
                    policy.Status = PolicyStatus.Rejected;
                    toUpdate.Add(policy);
                    await _notificationService.CreateNotificationAsync(
                        policy.CustomerId, 
                        $"Notice: Your policy '{policy.PolicyNumber}' has been deactivated as your assigned agent is no longer active."
                    );
                }
                if (toUpdate.Any()) await _policyRepo.UpdateRangeAsync(toUpdate);
            }
            else if (user.Role == UserRole.ClaimsOfficer)
            {
                var claims = await _claimRepo.GetByOfficerIdAsync(user.Id);
                var toUpdate = new List<Claim>();
                foreach (var claim in claims.Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview || c.Status == ClaimStatus.Approved))
                {
                    claim.RejectionReason = $"RECOVERY_OFFICER_OFFLINE|{claim.Status}";
                    claim.Status = ClaimStatus.Rejected;
                    toUpdate.Add(claim);
                    await _notificationService.CreateNotificationAsync(
                        claim.CustomerId, 
                        $"Notice: Your claim '{claim.ClaimNumber}' has been rejected as the processing officer is no longer active."
                    );
                }
                if (toUpdate.Any()) await _claimRepo.UpdateRangeAsync(toUpdate);
            }
            else if (user.Role == UserRole.Customer)
            {
                var policies = await _policyRepo.GetByCustomerIdAsync(user.Id);
                var toUpdatePolicies = new List<Policy>();
                foreach (var policy in policies.Where(p => p.Status == PolicyStatus.Active || p.Status == PolicyStatus.Submitted || p.Status == PolicyStatus.UnderReview || p.Status == PolicyStatus.DocumentsSubmitted || p.Status == PolicyStatus.Approved))
                {
                    policy.RejectionReason = $"RECOVERY_CUSTOMER_OFFLINE|{policy.Status}";
                    policy.Status = PolicyStatus.Rejected;
                    toUpdatePolicies.Add(policy);
                }
                if (toUpdatePolicies.Any()) await _policyRepo.UpdateRangeAsync(toUpdatePolicies);

                var claims = await _claimRepo.GetByCustomerIdAsync(user.Id);
                var toUpdateClaims = new List<Claim>();
                foreach (var claim in claims.Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview || c.Status == ClaimStatus.Approved))
                {
                    claim.RejectionReason = $"RECOVERY_CUSTOMER_OFFLINE|{claim.Status}";
                    claim.Status = ClaimStatus.Rejected;
                    toUpdateClaims.Add(claim);
                }
                if (toUpdateClaims.Any()) await _claimRepo.UpdateRangeAsync(toUpdateClaims);
            }
        }
        else // Reactivated
        {
            if (user.Role == UserRole.Agent)
            {
                var policies = await _policyRepo.GetByAgentIdAsync(user.Id);
                var toUpdate = new List<Policy>();
                foreach (var policy in policies.Where(p => p.Status == PolicyStatus.Rejected && p.RejectionReason != null && p.RejectionReason.StartsWith("RECOVERY_AGENT_OFFLINE")))
                {
                    var parts = policy.RejectionReason.Split('|');
                    if (parts.Length > 1 && Enum.TryParse<PolicyStatus>(parts[1], out var oldStatus))
                    {
                        policy.Status = oldStatus;
                        policy.RejectionReason = null;
                        toUpdate.Add(policy);
                        await _notificationService.CreateNotificationAsync(
                            policy.CustomerId,
                            $"Good news! Your policy '{policy.PolicyNumber}' has been restored as your assigned agent is back online."
                        );
                    }
                }
                if (toUpdate.Any()) await _policyRepo.UpdateRangeAsync(toUpdate);
            }
            else if (user.Role == UserRole.ClaimsOfficer)
            {
                var claims = await _claimRepo.GetByOfficerIdAsync(user.Id);
                var toUpdate = new List<Claim>();
                foreach (var claim in claims.Where(c => c.Status == ClaimStatus.Rejected && c.RejectionReason != null && c.RejectionReason.StartsWith("RECOVERY_OFFICER_OFFLINE")))
                {
                    var parts = claim.RejectionReason.Split('|');
                    if (parts.Length > 1 && Enum.TryParse<ClaimStatus>(parts[1], out var oldStatus))
                    {
                        claim.Status = oldStatus;
                        claim.RejectionReason = null;
                        toUpdate.Add(claim);
                        await _notificationService.CreateNotificationAsync(
                            claim.CustomerId,
                            $"Good news! Your claim '{claim.ClaimNumber}' is being processed again as the assigned officer is back online."
                        );
                    }
                }
                if (toUpdate.Any()) await _claimRepo.UpdateRangeAsync(toUpdate);
            }
            else if (user.Role == UserRole.Customer)
            {
                var policies = await _policyRepo.GetByCustomerIdAsync(user.Id);
                var toUpdatePolicies = new List<Policy>();
                foreach (var policy in policies.Where(p => p.Status == PolicyStatus.Rejected && p.RejectionReason != null && p.RejectionReason.StartsWith("RECOVERY_CUSTOMER_OFFLINE")))
                {
                    var parts = policy.RejectionReason.Split('|');
                    if (parts.Length > 1 && Enum.TryParse<PolicyStatus>(parts[1], out var oldStatus))
                    {
                        policy.Status = oldStatus;
                        policy.RejectionReason = null;
                        toUpdatePolicies.Add(policy);
                    }
                }
                if (toUpdatePolicies.Any()) await _policyRepo.UpdateRangeAsync(toUpdatePolicies);

                var claims = await _claimRepo.GetByCustomerIdAsync(user.Id);
                var toUpdateClaims = new List<Claim>();
                foreach (var claim in claims.Where(c => c.Status == ClaimStatus.Rejected && c.RejectionReason != null && c.RejectionReason.StartsWith("RECOVERY_CUSTOMER_OFFLINE")))
                {
                    var parts = claim.RejectionReason.Split('|');
                    if (parts.Length > 1 && Enum.TryParse<ClaimStatus>(parts[1], out var oldStatus))
                    {
                        claim.Status = oldStatus;
                        claim.RejectionReason = null;
                        toUpdateClaims.Add(claim);
                    }
                }
                if (toUpdateClaims.Any()) await _claimRepo.UpdateRangeAsync(toUpdateClaims);
            }
        }
    }

    // ── Private Helpers ────────────────────────────────────────────────────

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var token = _jwtService.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
    }

    private static UserListDto MapToUserListDto(User user) => new()
    {
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        IsDeleted = user.IsDeleted,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        DeletedAt = user.DeletedAt
    };

    private static string GenerateTemporaryPassword()
    {
        var random = new Random();
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "@$!%*?&";

        var password = new[]
        {
            upper[random.Next(upper.Length)],
            special[random.Next(special.Length)],
            lower[random.Next(lower.Length)],
            lower[random.Next(lower.Length)],
            digits[random.Next(digits.Length)],
            digits[random.Next(digits.Length)],
            lower[random.Next(lower.Length)],
            lower[random.Next(lower.Length)],
            digits[random.Next(digits.Length)],
            lower[random.Next(lower.Length)]
        };

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }

    public Task<StaffResponseDto> CreateStaffAsync(CreateStaffDto dto)
    {
        throw new NotImplementedException();
    }

    // ── Regular password change (any logged-in user) ────────────────────────

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        // Verify current password before allowing change
        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (dto.CurrentPassword == dto.NewPassword)
            throw new InvalidOperationException(
                "New password must be different from current password.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.MustChangePassword = false;
        await _userRepo.UpdateAsync(user);
    }

    // ── Forgot Password / Reset Password ──────────────────────────────────────

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user == null || !user.IsActive || user.IsDeleted)
        {
            // Do not reveal if the user exists but is inactive, for security
            return false;
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.MustChangePassword = false;
        
        await _userRepo.UpdateAsync(user);
        return true;
    }
}