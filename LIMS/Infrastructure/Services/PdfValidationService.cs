using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using FuzzySharp;
using UglyToad.PdfPig;

namespace Infrastructure.Services;

public class PdfValidationService : IPdfValidationService
{
    private readonly IClaimRepository _claimRepo;
    private readonly IPolicyRepository _policyRepo;

    public PdfValidationService(IClaimRepository claimRepo, IPolicyRepository policyRepo)
    {
        _claimRepo = claimRepo;
        _policyRepo = policyRepo;
    }

    public async Task<ClaimValidationResult> ValidateClaimAsync(int claimId)
    {
        var result = new ClaimValidationResult { IsValidationPerformed = false };
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId);
        
        if (claim == null) return result;
        if (claim.Type != ClaimType.Death) return result;
        
        result.IsValidationPerformed = true;

        // 1. Validate Date of Death (from Death Certificate)
        result.DateValidation = await PerformDateValidationAsync(claim);

        // 2. Validate Nominee ID Proof (matter inside must be same)
        result.NomineeIdValidation = await PerformNomineeIdProofValidationAsync(claim);

        return result;
    }

    public async Task<PolicyValidationResult> ValidatePolicyAsync(int policyId)
    {
        var result = new PolicyValidationResult { IsValidationPerformed = false };
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId);

        if (policy == null || policy.Customer == null) return result;

        result.IsValidationPerformed = true;

        // Validate Date of Birth (from Aadhar Card)
        result.DobValidation = await PerformCustomerDobValidationAsync(policy);

        return result;
    }

    private async Task<DateValidationResult> PerformCustomerDobValidationAsync(Policy policy)
    {
        var validation = new DateValidationResult();
        var targetDob = policy.Customer.DateOfBirth;
        validation.EnteredDate = targetDob.ToString("dd/MM/yyyy");

        // Identify Customer Aadhar Document
        var aadharDoc = policy.Documents?.FirstOrDefault(d => 
            d.DocumentType.Equals("Aadhar Card", StringComparison.OrdinalIgnoreCase) ||
            d.DocumentType.Equals("Address Proof", StringComparison.OrdinalIgnoreCase));

        if (aadharDoc == null)
        {
            validation.ErrorMessage = "No Aadhar Card document uploaded for the customer.";
            return validation;
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", aadharDoc.FilePath.TrimStart('/'));
        if (!File.Exists(filePath)) filePath = Path.Combine(Directory.GetCurrentDirectory(), "API", "wwwroot", aadharDoc.FilePath.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            validation.ErrorMessage = "Aadhar Card file not found on server.";
            return validation;
        }

        // Use the generic date validation internal helper
        var matchResult = await InternalValidateDateAsync(filePath, targetDob);
        
        validation.IsMatch = matchResult.IsMatch;
        validation.ExtractedDate = matchResult.ExtractedDate;
        validation.ErrorMessage = matchResult.ErrorMessage;

        if (!validation.IsMatch && string.IsNullOrEmpty(validation.ErrorMessage))
        {
             validation.ErrorMessage = $"DOB Mismatch: Policy record has '{validation.EnteredDate}' but Aadhar document has different dates.";
        }

        return validation;
    }

    private async Task<DateValidationResult> PerformDateValidationAsync(Claim claim)
    {
        var validation = new DateValidationResult();

        // Parse Date of Death from the claim reason
        DateTime? dateOfDeath = null;
        if (claim.ClaimReason.StartsWith("Date of Death: "))
        {
            var parts = claim.ClaimReason.Split('.');
            var dateStr = parts[0].Replace("Date of Death: ", "").Trim();
            if (DateTime.TryParse(dateStr, out var parsed)) dateOfDeath = parsed;
        }

        if (!dateOfDeath.HasValue)
        {
            validation.ErrorMessage = "Could not parse date of death from claim reason.";
            return validation;
        }

        validation.EnteredDate = dateOfDeath.Value.ToString("dd/MM/yyyy");

        // Get the Death Certificate PDF
        var deathCert = claim.ClaimDocuments?.FirstOrDefault(d => 
            d.DocumentType.Contains("Death", StringComparison.OrdinalIgnoreCase));

        if (deathCert == null)
        {
            validation.ErrorMessage = "No death certificate uploaded.";
            return validation;
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", deathCert.FilePath.TrimStart('/'));
        if (!File.Exists(filePath)) filePath = Path.Combine(Directory.GetCurrentDirectory(), "API", "wwwroot", deathCert.FilePath.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            validation.ErrorMessage = "Death certificate file not found on server.";
            return validation;
        }

        var result = await InternalValidateDateAsync(filePath, dateOfDeath.Value);
        validation.IsMatch = result.IsMatch;
        validation.ExtractedDate = result.ExtractedDate;
        validation.ErrorMessage = result.ErrorMessage;

        // Extra range checks
        var activeFrom = claim.Policy?.ActiveFrom?.Date;
        var submittedOn = claim.SubmittedAt.Date;

        if (activeFrom.HasValue && dateOfDeath.Value.Date < activeFrom.Value)
        {
            validation.IsMatch = false;
            validation.ErrorMessage += $" | Date of Death ({dateOfDeath.Value:dd/MM/yyyy}) is before policy active date ({activeFrom.Value:dd/MM/yyyy})";
        }
        else if (dateOfDeath.Value.Date > submittedOn)
        {
            validation.IsMatch = false;
            validation.ErrorMessage += $" | Date of Death ({dateOfDeath.Value:dd/MM/yyyy}) is after the claim submission date ({submittedOn:dd/MM/yyyy})";
        }

        return validation;
    }

    private async Task<NomineeIdValidationResult> PerformNomineeIdProofValidationAsync(Claim claim)
    {
        var validation = new NomineeIdValidationResult { IsMatch = false };

        // 1. Identify Nominee ID Proof from claim
        var claimDoc = claim.ClaimDocuments?.FirstOrDefault(d => 
            d.DocumentType.Equals("Nominee ID Proof", StringComparison.OrdinalIgnoreCase));

        if (claimDoc == null)
        {
            validation.ErrorMessage = "No Nominee ID Proof found in claim documents.";
            return validation;
        }

        var claimPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", claimDoc.FilePath.TrimStart('/'));
        if (!File.Exists(claimPath)) claimPath = Path.Combine(Directory.GetCurrentDirectory(), "API", "wwwroot", claimDoc.FilePath.TrimStart('/'));

        if (!File.Exists(claimPath))
        {
            validation.ErrorMessage = "Nominee ID proof file is missing on the server.";
            return validation;
        }

        if (string.IsNullOrEmpty(claim.NomineeIdNumber))
        {
             validation.ErrorMessage = "No Nominee ID Number found in claim record to verify.";
             return validation;
        }

        // ── POSITIVE MATCH INTERCEPT: IDENTICAL NOMINEE PROOFS ──────────
        // If the claimant uploaded the literal exact same file as the original policy's Nominee Proof, instantly pass!
        var policyNomineeDoc = claim.Policy?.Documents?.FirstOrDefault(d => d.DocumentType.Equals("Nominee ID Proof", StringComparison.OrdinalIgnoreCase));
        if (policyNomineeDoc != null)
        {
            var rootDirFast = Directory.GetCurrentDirectory();
            var policyNomPath = Path.Combine(rootDirFast, "wwwroot", policyNomineeDoc.FilePath.TrimStart('/'));
            if (!File.Exists(policyNomPath)) policyNomPath = Path.Combine(rootDirFast, "API", "wwwroot", policyNomineeDoc.FilePath.TrimStart('/'));

            if (File.Exists(policyNomPath))
            {
                try 
                {
                    var claimBytes = File.ReadAllBytes(claimPath);
                    var policyNomBytes = File.ReadAllBytes(policyNomPath);
                    if (claimBytes.Length == policyNomBytes.Length && claimBytes.SequenceEqual(policyNomBytes))
                    {
                        validation.IsMatch = true;
                        return validation; // Bypass OCR completely because the physical identity document is undeniably the original registered one!
                    }
                }
                catch { }
            }
        }

        // ── SECURITY CHECK INITIAL INTERCEPT: INVALID FILE MATCH ──────────
        var customerAadharDocFast = claim.Policy?.Documents?.FirstOrDefault(d => 
            d.DocumentType.Equals("Aadhar Card", StringComparison.OrdinalIgnoreCase) ||
            d.DocumentType.Equals("Address Proof", StringComparison.OrdinalIgnoreCase));

        if (customerAadharDocFast != null)
        {
            var rootDirFast = Directory.GetCurrentDirectory();
            var customerPathFast = Path.Combine(rootDirFast, "wwwroot", customerAadharDocFast.FilePath.TrimStart('/'));
            if (!File.Exists(customerPathFast)) customerPathFast = Path.Combine(rootDirFast, "API", "wwwroot", customerAadharDocFast.FilePath.TrimStart('/'));

            if (File.Exists(customerPathFast))
            {
                // If they explicitly submitted the exact identical file to the customer's own file, instantly block it!
                try 
                {
                    var claimBytes = File.ReadAllBytes(claimPath);
                    var customerBytes = File.ReadAllBytes(customerPathFast);
                    if (claimBytes.Length == customerBytes.Length && claimBytes.SequenceEqual(customerBytes))
                    {
                        validation.IsMatch = false;
                        validation.ErrorMessage = "SECURITY ALERT: Nominee Aadhar matches the Customer's Aadhar document exactly — Invalid Identity.";
                        return validation;
                    }
                }
                catch { }
            }
        }

        try
        {
            var claimText = GetPdfText(claimPath);

            if (string.IsNullOrWhiteSpace(claimText))
            {
                validation.ErrorMessage = "Could not extract text from the ID proof document.";
                return validation;
            }

            // Aadhar numbers in India are almost universally printed with spaces e.g. '1234 5678 9012'.
            // This modified regex matches 12 digits regardless of interleaving spaces or hyphens.
            var aadharMatches = Regex.Matches(claimText, @"\b(?:\d[\s-]*){12}\b");
            var nomineeAadharNumbers = aadharMatches.Select(m => Regex.Replace(m.Value, @"[\s-]", "")).ToList();

            if (!nomineeAadharNumbers.Any())
            {
                validation.ErrorMessage = "No recognizable 12-digit Aadhar block found inside the Nominee ID document.";
                return validation;
            }

            // Check if entered number is among the safely stripped numeric fragments
            var enteredNomineeId = claim.NomineeIdNumber?.Replace(" ", "")?.Replace("-", "")?.Trim();
            validation.IsMatch = nomineeAadharNumbers.Any(n => n == enteredNomineeId);
            
            if (!validation.IsMatch)
            {
                var foundAny = nomineeAadharNumbers.FirstOrDefault();
                validation.ErrorMessage = $"Nominee Aadhar Mismatch: Entered '{enteredNomineeId}' but found '{foundAny}' in the document.";
                return validation;
            }

            // ── SECURITY CHECK: Nominee Aadhar must be DIFFERENT from Customer Aadhar ──────────
            // Find the Customer's Aadhar document from the policy records
            var customerAadharDoc = claim.Policy?.Documents?.FirstOrDefault(d => 
                d.DocumentType.Equals("Aadhar Card", StringComparison.OrdinalIgnoreCase) ||
                d.DocumentType.Equals("Address Proof", StringComparison.OrdinalIgnoreCase));

            if (customerAadharDoc != null)
            {
            var rootDir = Directory.GetCurrentDirectory();
            var customerPath = Path.Combine(rootDir, "wwwroot", customerAadharDoc.FilePath.TrimStart('/'));
            if (!File.Exists(customerPath)) customerPath = Path.Combine(rootDir, "API", "wwwroot", customerAadharDoc.FilePath.TrimStart('/'));

            if (File.Exists(customerPath))
                {
                    try
                    {
                        var customerText = GetPdfText(customerPath);
                        var customerAadharNumbers = Regex.Matches(customerText, @"\b(?:\d[\s-]*){12}\b")
                            .Select(m => Regex.Replace(m.Value, @"[\s-]", "")).ToList();

                        // If the customer document contains the nominee's Aadhar number — BLOCK IT
                        if (customerAadharNumbers.Any(cn => nomineeAadharNumbers.Any(nn => nn == cn)))
                        {
                            validation.IsMatch = false;
                            validation.ErrorMessage = "SECURITY ALERT: Nominee Aadhar number matches the Customer's Aadhar number — Invalid Identity.";
                            return validation;
                        }
                    }
                    catch { /* Extract fail on customer doc is not a blocker for nominee match, but we prefer checking it */ }
                }
            }
            // ──────────────────────────────────────────────────────────────────────────────────

            return validation;
        }
        catch (Exception ex)
        {
            validation.ErrorMessage = $"Error verifying identity in PDF: {ex.Message}";
            return validation;
        }
    }

    private string GetPdfText(string filePath)
    {
        string text = "";
        try
        {
            using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath, new UglyToad.PdfPig.ParsingOptions { UseLenientParsing = true }))
            {
                foreach (var page in document.GetPages())
                {
                    try
                    {
                        // Fallback 1: Deep letter extraction (handles highly fragmented vectors)
                        text += string.Join("", page.Letters.Select(l => l.Value)) + " ";
                    }
                    catch { }

                    try
                    {
                        // Fallback 2: Processable word groups
                        text += string.Join(" ", page.GetWords().Select(w => w.Text)) + " ";
                    }
                    catch { }
                    
                    try
                    {
                        // Default bounding box
                        text += page.Text + " ";
                    }
                    catch { }
                }
            }
        }
        catch { }
        
        return text;
    }

    private async Task<PdfValidationResult> InternalValidateDateAsync(string filePath, DateTime targetDate)
    {
        var result = new PdfValidationResult
        {
            EnteredDate = targetDate.ToString("dd/MM/yyyy"),
            IsMatch = false
        };

        try
        {
            string pdfText = GetPdfText(filePath);
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
                var cleanStr = str.TrimEnd(',', '.');

                if (DateTime.TryParse(cleanStr, out var parsedDate))
                {
                    if (parsedDate.Date == targetDate.Date)
                    {
                        result.IsMatch = true;
                        result.ExtractedDate = cleanStr;
                        return result;
                    }
                    bestMatchString = cleanStr;
                }
                else 
                {
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
