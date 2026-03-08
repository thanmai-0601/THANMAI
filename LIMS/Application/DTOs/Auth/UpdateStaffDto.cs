using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class UpdateStaffDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }  // Admin can change role here too
}