namespace Application.DTOs.Policy;

public class PolicyResponseDto
{
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Plan info
    public int InsurancePlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }

    // Customer info
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    // Agent info (null until assigned)
    public int? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string? AgentEmail { get; set; }

    // Eligibility details (filled by Agent)
    public int? CustomerAge { get; set; }
    public decimal? AnnualIncome { get; set; }
    public string? Occupation { get; set; }
    public string? RiskCategory { get; set; }
    public decimal? PremiumAmount { get; set; }
    public string? AgentRemarks { get; set; }
    public string? RejectionReason { get; set; }

    // Dates
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AgentAssignedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }

    // Collections
    public List<NomineeResponseDto> Nominees { get; set; } = new();
    public List<DocumentResponseDto> Documents { get; set; } = new();
    public bool HasSettledClaim { get; set; }
}