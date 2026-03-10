using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

public class AddNomineeDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Relationship { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Age { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be exactly 10 digits")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhar ID must be exactly 12 digits")]
    public string IdNumber { get; set; } = string.Empty; // 12-digit Aadhar number

    [Required]
    [EmailAddress(ErrorMessage = "Invalid nominee email address")]
    public string Email { get; set; } = string.Empty;
}