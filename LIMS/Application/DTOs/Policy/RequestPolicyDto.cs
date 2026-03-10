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
    [Range(1, double.MaxValue, ErrorMessage = "Annual income must be greater than 0")]
    public decimal AnnualIncome { get; set; }

    [Required]
    [StringLength(100)]
    public string Occupation { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;

    // ── Upfront Nominee and Documents — required for review ──────────
    
    [Required]
    public AddNomineeDto Nominee { get; set; } = new();

    [Required]
    [MinLength(3, ErrorMessage = "Three major documents are required: Address Proof, Income Proof, Nominee ID Proof.")]
    public List<UploadDocumentDto> Documents { get; set; } = new();
}