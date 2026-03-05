using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class PolicyEndorsement : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public int RequestedByCustomerId { get; set; }
    public User RequestedByCustomer { get; set; } = null!;

    public int? ReviewedByAgentId { get; set; }
    public User? ReviewedByAgent { get; set; }

    public EndorsementType Type { get; set; }
    public EndorsementStatus Status { get; set; } = EndorsementStatus.Requested;

    // Stores what the customer wants to change — JSON string
    // e.g. for AddressChange: {"newAddress": "123 Main St"}
    // e.g. for SumAssuredIncrease: {"newSumAssured": 2000000}
    public string ChangeRequestJson { get; set; } = string.Empty;

    // Snapshot of old value before change (for audit trail)
    public string OldValueJson { get; set; } = string.Empty;

    public string? AgentRemarks { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}