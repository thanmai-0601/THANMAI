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
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 100, ErrorMessage = "Allocation percentage must be between 1 and 100")]
    public decimal AllocationPercentage { get; set; }
}