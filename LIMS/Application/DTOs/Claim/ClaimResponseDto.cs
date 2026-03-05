namespace Application.DTOs.Claim;

public class ClaimResponseDto
{
    public int ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Policy info
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal SumAssured { get; set; }
    public int TenureYears { get; set; }
    public string? RiskCategory { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime? PolicyActiveFrom { get; set; }
    public DateTime? PolicyActiveTo { get; set; }

    // Customer info
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    // Claims Officer info
    public int? ClaimsOfficerId { get; set; }
    public string? ClaimsOfficerName { get; set; }

    // Claim details
    public string ClaimReason { get; set; } = string.Empty;
    public decimal? ClaimAmount { get; set; }
    public decimal? SettledAmount { get; set; }
    public string? OfficerRemarks { get; set; }
    public string? RejectionReason { get; set; }

    // Bank details
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIfscCode { get; set; }
    public string? TransferReference { get; set; }

    // Nominees (from the policy)
    public List<ClaimNomineeDto> Nominees { get; set; } = new();

    // Payment summary
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal TotalPremiumPaid { get; set; }

    // Documents
    public List<ClaimDocumentResponseDto> Documents { get; set; } = new();

    // Dates
    public DateTime SubmittedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? ReviewStartedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime? RejectedAt { get; set; }
}

public class ClaimNomineeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
}