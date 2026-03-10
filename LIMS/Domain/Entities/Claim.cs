using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Claim : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int? ClaimsOfficerId { get; set; }      // assigned after submission
    public User? ClaimsOfficer { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public ClaimType Type { get; set; } = ClaimType.Death;

    public string ClaimReason { get; set; } = string.Empty;
    public string? OfficerRemarks { get; set; }
    public string? RejectionReason { get; set; }

    public decimal? ClaimAmount { get; set; }       // amount approved for settlement
    public decimal? SettledAmount { get; set; }     // actual amount paid out

    // ── Claimant bank details for settlement transfer ──────────────────
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIfscCode { get; set; }
    public string? TransferReference { get; set; }  // filled on settlement

    // Key dates
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AssignedAt { get; set; }
    public DateTime? ReviewStartedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    // Navigation
    public ICollection<ClaimDocument> ClaimDocuments { get; set; }
        = new List<ClaimDocument>();
}