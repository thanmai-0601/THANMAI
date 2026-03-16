using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using FuzzySharp;
using UglyToad.PdfPig;

namespace Infrastructure.Services;

public class PdfValidationService : IPdfValidationService
{
    private readonly IClaimRepository _claimRepo;

    public PdfValidationService(IClaimRepository claimRepo)
    {
        _claimRepo = claimRepo;
    }

    public async Task<PdfValidationResult> ValidateClaimDateAsync(int claimId)
    {
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId);
        if (claim == null) return new PdfValidationResult { ErrorMessage = "Claim not found." };

        // Only validate if it's a death claim (maturity claims don't have docs to validate against in this scope yet)
        if (claim.Type != ClaimType.Death)
        {
            return new PdfValidationResult { IsValidationPerformed = false };
        }

        // Identify the target date (Date of Death)
        DateTime? dateOfDeath = null;
        if (claim.ClaimReason.StartsWith("Date of Death: "))
        {
            var parts = claim.ClaimReason.Split('.');
            var dateStr = parts[0].Replace("Date of Death: ", "").Trim();
            if (DateTime.TryParse(dateStr, out var parsed)) dateOfDeath = parsed;
        }

        if (!dateOfDeath.HasValue)
        {
            return new PdfValidationResult { IsValidationPerformed = true, IsMatch = false, ErrorMessage = "Could not parse date of death from claim reason." };
        }

        // Identify the target document (Death Certificate)
        var deathCert = claim.ClaimDocuments?.FirstOrDefault(d => 
            d.DocumentType.Contains("Death", StringComparison.OrdinalIgnoreCase) || 
            d.DocumentType == "DeathCertificate");

        if (deathCert == null)
        {
            return new PdfValidationResult { IsValidationPerformed = true, IsMatch = false, ErrorMessage = "No death certificate uploaded." };
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", deathCert.FilePath.TrimStart('/'));
        var result = await ValidateDateAsync(filePath, dateOfDeath.Value);
        result.IsValidationPerformed = true;

        // Range Validation (User Request: "on or after taking policy date and on or before raising claim date")
        var activeFrom = claim.Policy?.ActiveFrom?.Date;
        var submittedOn = claim.SubmittedAt.Date;

        if (activeFrom.HasValue && dateOfDeath.Value.Date < activeFrom.Value)
        {
            result.IsMatch = false;
            result.ErrorMessage = $"rejected due to wrong details submitted: Date of Death ({dateOfDeath.Value:dd/MM/yyyy}) is before policy active date ({activeFrom.Value:dd/MM/yyyy})";
        }
        else if (dateOfDeath.Value.Date > submittedOn)
        {
            result.IsMatch = false;
            result.ErrorMessage = $"rejected due to wrong details submitted: Date of Death ({dateOfDeath.Value:dd/MM/yyyy}) is after the claim submission date ({submittedOn:dd/MM/yyyy})";
        }

        // Cross-Claim Consistency (Requirement 3: Match details of the first settled claim for this customer)
        var clientClaims = await _claimRepo.GetByCustomerIdAsync(claim.CustomerId);
        var firstResolved = clientClaims.FirstOrDefault(c => 
            c.Id != claim.Id && 
            c.Type == ClaimType.Death && 
            (c.Status == ClaimStatus.Settled || c.Status == ClaimStatus.Approved));

        if (firstResolved != null && firstResolved.ClaimReason != claim.ClaimReason)
        {
            result.IsMatch = false;
            result.ErrorMessage = $"rejected due to wrong details submitted: Death details (Date/Cause) do not match the previously settled claim '{firstResolved.ClaimNumber}'.";
        }

        // Auto-Reject removed as per user request: "if dates are not matched then automatic rejection should not happen but approval thing should be disabled"
        // if (!result.IsMatch && claim.Status != ClaimStatus.Rejected && claim.Status != ClaimStatus.Settled)
        // {
        //     claim.Status = ClaimStatus.Rejected;
        //     claim.RejectedAt = DateTime.UtcNow;
        //     claim.RejectionReason = "rejected due to wrong details submitted";
        //     claim.OfficerRemarks = "System Auto-Rejection: " + result.ErrorMessage;
        //     await _claimRepo.UpdateAsync(claim);
        // }

        return result;
    }

    public async Task<PdfValidationResult> ValidateDateAsync(string filePath, DateTime targetDate)
    {
        var result = new PdfValidationResult
        {
            EnteredDate = targetDate.ToString("dd/MM/yyyy"),
            IsMatch = false
        };

        if (!File.Exists(filePath))
        {
            result.ErrorMessage = "Document file not found.";
            return result;
        }

        try
        {
            string pdfText = "";
            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    pdfText += page.Text + " ";
                }
            }

            if (string.IsNullOrWhiteSpace(pdfText))
            {
                result.ErrorMessage = "No text could be extracted from the PDF.";
                return result;
            }

            // Extract potential date strings using Regex
            var datePatterns = new[]
            {
                @"\d{1,2}[/-]\d{1,2}[/-]\d{2,4}", // dd/MM/yyyy or dd-MM-yyyy
                @"\d{1,2}\.\d{1,2}\.\d{2,4}",     // dd.MM.yyyy
                @"\d{4}-\d{1,2}-\d{1,2}",         // yyyy-MM-dd
                @"\d{1,2}\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{2,4}", // 15 March 2024
                @"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{1,2},?\s+\d{2,4}" // March 15, 2024
            };

            var extractedStrings = new List<string>();
            foreach (var pattern in datePatterns)
            {
                var matches = Regex.Matches(pdfText, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    extractedStrings.Add(match.Value.Trim());
                }
            }

            if (!extractedStrings.Any())
            {
                result.ErrorMessage = "No dates found in the uploaded document.";
                return result;
            }

            // Strict Validation: Parse extracted strings into DateTime and compare
            // This prevents "12/03/2026" from matching "13/03/2026"
            DateTime? bestMatchDate = null;
            string bestMatchString = "";

            var parseFormats = new[] 
            { 
                "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy", "d.M.yyyy", "dd.MM.yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd",
                "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy",
                "MMM d yyyy", "MMM dd yyyy", "MMMM d yyyy", "MMMM dd yyyy"
            };

            foreach (var str in extractedStrings)
            {
                // Clean common OCR noise like extra commas or periods at the end
                var cleanStr = str.TrimEnd(',', '.');

                if (DateTime.TryParse(cleanStr, out var parsedDate))
                {
                    if (parsedDate.Date == targetDate.Date)
                    {
                        result.IsMatch = true;
                        result.ExtractedDate = cleanStr;
                        return result;
                    }
                    
                    // Keep track of the "closest" looking string for the error message
                    bestMatchString = cleanStr;
                }
                else 
                {
                    // If parsing fails, try ParseExact with common formats
                    if (DateTime.TryParseExact(cleanStr, parseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactParsed))
                    {
                        if (exactParsed.Date == targetDate.Date)
                        {
                            result.IsMatch = true;
                            result.ExtractedDate = cleanStr;
                            return result;
                        }
                        bestMatchString = cleanStr;
                    }
                }
            }

            // If we reached here, no exact match was found
            result.IsMatch = false;
            result.ExtractedDate = string.IsNullOrEmpty(bestMatchString) ? "No valid date found" : bestMatchString;
            result.ErrorMessage = $"Date mismatch: The PDF contains '{result.ExtractedDate}' but the claimant entered '{result.EnteredDate}'.";

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error during PDF processing: {ex.Message}";
            return result;
        }
    }
}
