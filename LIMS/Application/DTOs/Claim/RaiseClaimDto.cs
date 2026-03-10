using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Claim;

public class RaiseClaimDto
{
    [Required]
    public string PolicyNumber { get; set; } = string.Empty;

    public string? ClaimType { get; set; } = "Death";

    public string? CauseOfDeath { get; set; }

    public DateTime? DateOfDeath { get; set; }

    // ── Claimant (Nominee) Details ────────────────────────────────────
    public string? NomineeName { get; set; }

    public string? NomineeRelationship { get; set; }
    
    [Required]
    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Nominee ID must be exactly 12 digits")]
    public string? NomineeIdNumber { get; set; }

    // Uploaded ID proof for verification
    public ClaimDocumentDto? NomineeIdProof { get; set; }

    // ── Claimant bank details for settlement ──────────────────────────
    [Required]
    public string BankAccountName { get; set; } = string.Empty;

    [Required]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC code format (must be 11 characters)")]
    public string BankIfscCode { get; set; } = string.Empty;

    public ClaimDocumentDto? DeathCertificate { get; set; }
}