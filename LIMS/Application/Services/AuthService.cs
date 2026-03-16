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
    private readonly IAgentAssignmentService _agentAssignmentService;
    private readonly IClaimsOfficerAssignmentService _claimsOfficerAssignmentService;

    public AuthService(
        IUserRepository userRepo, 
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        IPolicyRepository policyRepo,
        IClaimRepository claimRepo,
        INotificationService notificationService,
        IAgentAssignmentService agentAssignmentService,
        IClaimsOfficerAssignmentService claimsOfficerAssignmentService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _policyRepo = policyRepo;
        _claimRepo = claimRepo;
        _notificationService = notificationService;
        _agentAssignmentService = agentAssignmentService;
        _claimsOfficerAssignmentService = claimsOfficerAssignmentService;
    }

    // ── Public Registration — always Customer ──────────────────────────────

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var age = DateTime.UtcNow.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;

        if (age < 18)
            throw new InvalidOperationException("not eligible as age is less");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Role = UserRole.Customer,  // always Customer, no exceptions
            IsActive = true,
            BankAccountName = dto.BankAccountName,
            BankAccountNumber = dto.BankAccountNumber,
            BankIfscCode = dto.BankIfscCode
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

        var age = DateTime.UtcNow.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;

        if (age < 18)
            throw new InvalidOperationException("Staff must be at least 18 years old.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Role = role,
            IsActive = true,
            MustChangePassword = false,
            BankAccountName = dto.BankAccountName,
            BankAccountNumber = dto.BankAccountNumber,
            BankIfscCode = dto.BankIfscCode
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

        // Reassign policies/claims if it's a staff member
        await HandleDeactivationTasksAsync(user);
    }

    // ── Admin: Toggle User Status ──────────────────────────────────────────

    private async Task HandleDeactivationTasksAsync(User user)
    {
        if (user.Role == UserRole.Agent)
        {
            var policies = await _policyRepo.GetByAgentIdAsync(user.Id);
            var toUpdate = new List<Policy>();

            // If policy is rejected, no new agent assignment takes place (it stays rejected with the old agent)
            // For all other states, reassignment takes place
            foreach (var policy in policies.Where(p => p.Status != PolicyStatus.Rejected))
            {
                try
                {
                    var newAgentId = await _agentAssignmentService.AssignAgentAsync();
                    policy.AgentId = newAgentId;
                    policy.AgentAssignedAt = DateTime.UtcNow;

                    toUpdate.Add(policy);

                    await _notificationService.CreateNotificationAsync(
                        policy.CustomerId,
                        $"Notice: Your policy '{policy.PolicyNumber}' has been reassigned to a new agent as your previous agent is no longer active."
                    );
                }
                catch (InvalidOperationException)
                {
                    // No other active agents available
                }
            }
            if (toUpdate.Any()) await _policyRepo.UpdateRangeAsync(toUpdate);
        }
        else if (user.Role == UserRole.ClaimsOfficer)
        {
            var claims = await _claimRepo.GetByOfficerIdAsync(user.Id);
            var toUpdate = new List<Claim>();

            // If the claim is settled then reassignment is not done, in other states it is done
            foreach (var claim in claims.Where(c => c.Status != ClaimStatus.Settled))
            {
                try
                {
                    var newOfficerId = await _claimsOfficerAssignmentService.AssignOfficerAsync();
                    claim.ClaimsOfficerId = newOfficerId;
                    claim.AssignedAt = DateTime.UtcNow;

                    toUpdate.Add(claim);

                    await _notificationService.CreateNotificationAsync(
                        claim.CustomerId,
                        $"Notice: Your claim '{claim.ClaimNumber}' has been reassigned to a new claims officer."
                    );
                }
                catch (InvalidOperationException)
                {
                    // No other active officers available
                }
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

    public async Task ToggleUserStatusAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin account status cannot be toggled.");

        if (user.Role == UserRole.Customer)
            throw new InvalidOperationException("Customer account status cannot be toggled manually. This is handled by automated system processes (e.g., policy settlements).");

        user.IsActive = !user.IsActive;
        await _userRepo.UpdateAsync(user);

        // If deactivated, handle related policies and claims
        if (!user.IsActive)
        {
            await HandleDeactivationTasksAsync(user);
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

    // ── Profile Management ───────────────────────────────────────────────────

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        return MapToUserProfileDto(user);
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        if (dto.DateOfBirth > DateTime.UtcNow)
            throw new InvalidOperationException("Date of Birth cannot be in the future.");

        user.DateOfBirth = dto.DateOfBirth;
        user.BankAccountName = dto.BankAccountName;
        user.BankAccountNumber = dto.BankAccountNumber;
        user.BankIfscCode = dto.BankIfscCode;

        await _userRepo.UpdateAsync(user);
    }

    private static UserProfileDto MapToUserProfileDto(User user) => new()
    {
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        DateOfBirth = user.DateOfBirth,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        BankAccountName = user.BankAccountName,
        BankAccountNumber = user.BankAccountNumber,
        BankIfscCode = user.BankIfscCode
    };

}