using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Agent approves or rejects
public class PolicyDecisionDto
{
    [Required]
    public bool IsApproved { get; set; }

    // Required only when rejecting
    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public string? RiskCategory { get; set; }
    public string? AgentRemarks { get; set; }
}