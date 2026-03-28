namespace Application.Interfaces.Services;

public interface IPdfValidationService
{
    Task<ClaimValidationResult> ValidateClaimAsync(int claimId);
    Task<PolicyValidationResult> ValidatePolicyAsync(int policyId);
}

public class PolicyValidationResult
{
    public bool IsValidationPerformed { get; set; }
    public DateValidationResult DobValidation { get; set; } = new();
    public string SummaryErrorMessage => DobValidation.ErrorMessage;
}

public class ClaimValidationResult
{
    public bool IsValidationPerformed { get; set; }
    public DateValidationResult DateValidation { get; set; } = new();
    public NomineeIdValidationResult NomineeIdValidation { get; set; } = new();
    public string SummaryErrorMessage => DateValidation.ErrorMessage + (string.IsNullOrEmpty(NomineeIdValidation.ErrorMessage) ? "" : " | " + NomineeIdValidation.ErrorMessage);
}

public class DateValidationResult
{
    public bool IsMatch { get; set; }
    public string EnteredDate { get; set; } = string.Empty;
    public string ExtractedDate { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class NomineeIdValidationResult
{
    public bool IsMatch { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class PdfValidationResult
{
    public bool IsValidationPerformed { get; set; }
    public bool IsMatch { get; set; }
    public string EnteredDate { get; set; } = string.Empty;
    public string ExtractedDate { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
