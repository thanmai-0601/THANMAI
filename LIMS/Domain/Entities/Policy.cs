using Domain.Common;
using Domain.Entities;
using Domain.Enums;


namespace Domain.Entities;

public class Policy : BaseEntity
{
    // Auto-generated policy number e.g. "POL-20240001"
    public string PolicyNumber { get; set; } = string.Empty;

    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    // ── Relationships ───────────────────────────────────────────────
    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int InsurancePlanId { get; set; }
    public InsurancePlan InsurancePlan { get; set; } = null!;

    public int? AgentId { get; set; }          // nullable — assigned after submission
    public User? Agent { get; set; }

    // ── Plan Details captured at time of request ────────────────────
    // We store these here because plan details might change later
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }
    
    // Comma-separated list of selected riders
    public string SelectedRiders { get; set; } = string.Empty;

    // ── Agent fills these after eligibility evaluation ──────────────
    public int? CustomerAge { get; set; }
    public decimal? AnnualIncome { get; set; }
    public string? Occupation { get; set; }
    public string? RiskCategory { get; set; }  // "Low", "Medium", "High"
    public decimal? PremiumAmount { get; set; }

    // ── Key dates ───────────────────────────────────────────────────
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AgentAssignedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }
    public string? AgentRemarks { get; set; }
    public string? CustomerAddress { get; set; }

    // ── Navigation ──────────────────────────────────────────────────
    public ICollection<Nominee> Nominees { get; set; } = new List<Nominee>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<PolicyEndorsement> Endorsements { get; set; }
    = new List<PolicyEndorsement>();
}