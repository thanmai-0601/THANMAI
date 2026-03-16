namespace Application.Interfaces.Services;

public interface IPdfValidationService
{
    Task<PdfValidationResult> ValidateClaimDateAsync(int claimId);
}

public class PdfValidationResult
{
    public bool IsValidationPerformed { get; set; }
    public bool IsMatch { get; set; }
    public string EnteredDate { get; set; } = string.Empty;
    public string ExtractedDate { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
