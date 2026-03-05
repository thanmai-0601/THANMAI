using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Customer fills this when clicking Enroll on a plan
public class RequestPolicyDto
{
    [Required]
    public int InsurancePlanId { get; set; }

    [Required]
    [Range(100000, 50000000)]
    public decimal SumAssured { get; set; }

    [Required]
    [Range(5, 40)]
    public int TenureYears { get; set; }

    // ── Basic personal details submitted at enrollment ──────────────
    [Required]
    [Range(18, 70, ErrorMessage = "Age must be between 18 and 70")]
    public int CustomerAge { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Annual income must be greater than 0")]
    public decimal AnnualIncome { get; set; }

    [Required]
    [StringLength(100)]
    public string Occupation { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;

    // Optional list of selected riders from the plan
    public string SelectedRiders { get; set; } = string.Empty;
}