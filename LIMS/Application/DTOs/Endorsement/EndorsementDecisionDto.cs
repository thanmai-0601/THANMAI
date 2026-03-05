using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Endorsement;

public class EndorsementDecisionDto
{
    [Required]
    public bool IsApproved { get; set; }

    [StringLength(500)]
    public string? AgentRemarks { get; set; }

    // Required only when rejecting
    [StringLength(500)]
    public string? RejectionReason { get; set; }
}