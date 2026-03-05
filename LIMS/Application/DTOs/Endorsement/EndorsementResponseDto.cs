namespace Application.DTOs.Endorsement;

public class EndorsementResponseDto
{
    public int EndorsementId { get; set; }
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // What the customer requested (parsed for display)
    public string ChangeRequested { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string? AgentName { get; set; }
    public string? AgentRemarks { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}