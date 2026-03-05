using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Claim;

public class ClaimDecisionDto
{
    [Required]
    public bool IsApproved { get; set; }

    // Required when approving — confirmed settlement amount
    [Range(1, double.MaxValue)]
    public decimal? SettledAmount { get; set; }

    [StringLength(500)]
    public string? OfficerRemarks { get; set; }

    // Required when rejecting
    [StringLength(500)]
    public string? RejectionReason { get; set; }
}