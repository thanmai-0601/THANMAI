using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Claim;

public class RaiseClaimDto
{
    [Required]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    public string CauseOfDeath { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfDeath { get; set; }

    // ── Claimant (Nominee) Details ────────────────────────────────────
    [Required]
    public string NomineeName { get; set; } = string.Empty;

    [Required]
    public string NomineeRelationship { get; set; } = string.Empty;

    // ── Claimant bank details for settlement ──────────────────────────
    [Required]
    public string BankAccountName { get; set; } = string.Empty;

    [Required]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Required]
    public string BankIfscCode { get; set; } = string.Empty;

    [Required]
    public ClaimDocumentDto DeathCertificate { get; set; } = null!;
}