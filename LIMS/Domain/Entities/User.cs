using Domain.Common;
using Domain.Entities;
using Domain.Enums;


namespace Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool MustChangePassword { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;      // ← soft delete flag
    public DateTime? DeletedAt { get; set; }          // ← when it was deleted
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<Policy> CustomerPolicies { get; set; } = new List<Policy>();
    public ICollection<Policy> AgentPolicies { get; set; } = new List<Policy>();
}