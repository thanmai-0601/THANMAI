using Application.Interfaces.Services;
using Google.Cloud.AIPlatform.V1;

namespace Infrastructure.Services;

public class ClaimSummaryService : IClaimSummaryService
{
    private readonly IClaimService _claimService;

    public ClaimSummaryService(IClaimService claimService)
    {
        _claimService = claimService;
    }

    public async Task<string> GenerateClaimSummaryAsync(int claimId)
    {
        var claimDetails = await _claimService.GetClaimDetailsAsync(claimId, 0, "Admin");
        var prompt = $@"Generate a clear, professional insurance claim summary as a single well-structured paragraph.

Strict Requirements:
- Output MUST be exactly one continuous paragraph (no line breaks).
- Word count MUST be between 150 and 180 words.
- Do NOT use bullet points, headings, markdown, or special formatting.
- Do NOT include conversational filler phrases.
- Use formal, concise, and natural business language.

Formatting Rules:
- Do NOT wrap field labels in quotes.
- Only wrap important values in double quotes, such as claim number, policy number, email, and status (e.g., ""CLM12345"").
- Keep the paragraph smooth and readable, not robotic.

Content Requirements:
- Include all key details: claim number, policy number, customer name, email, submitted date, reason, claim amount, and status.
- Maintain a logical flow: claim identification → customer details → claim reason → financial details → current status → processing context.

Tone Guidelines:
- Avoid repetition and unnecessary words.
- Prefer shorter, clear sentences over long complex ones.
- Ensure it reads like a professional insurance report.

Claim Details:
Claim Number: {claimDetails.ClaimNumber}
Policy Number: {claimDetails.PolicyNumber}
Reason: {claimDetails.ClaimReason}
Claim Amount: {claimDetails.ClaimAmount}
Status: {claimDetails.Status}
Submitted Date: {claimDetails.SubmittedAt}
Customer Name: {claimDetails.CustomerName}
Email: {claimDetails.CustomerEmail}";

        // ── Extracted Text Injection ──
        string rawDocumentText = "";
        try
        {
            if (claimDetails.Documents != null && claimDetails.Documents.Any())
            {
                var rootDir = Directory.GetCurrentDirectory();
                foreach (var doc in claimDetails.Documents)
                {
                    if (doc.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        var pdfPath = Path.Combine(rootDir, "wwwroot", doc.FilePath.TrimStart('/'));
                        if (!File.Exists(pdfPath)) pdfPath = Path.Combine(rootDir, "API", "wwwroot", doc.FilePath.TrimStart('/'));
                        
                        if (File.Exists(pdfPath))
                        {
                            try 
                            { 
                                using var pdf = UglyToad.PdfPig.PdfDocument.Open(pdfPath);
                                rawDocumentText += $"\n--- BEGIN Extracted Text from '{doc.DocumentType}' ---\n";
                                var maxPages = Math.Min(3, pdf.NumberOfPages); // limit token cost
                                for (var i = 1; i <= maxPages; i++)
                                {
                                    rawDocumentText += pdf.GetPage(i).Text + "\n";
                                }
                                rawDocumentText += $"--- END Extracted Text from '{doc.DocumentType}' ---\n";
                            } 
                            catch { }
                        }
                    }
                }
            }
        }
        catch { }

        if (!string.IsNullOrWhiteSpace(rawDocumentText))
        {
            prompt += "\n\nCRITICAL AI VALIDATION: MISMATCH DETECTION" +
                      "\nBelow is the raw text extracted from the customer's uploaded PDF claim documents." +
                      $"\n{rawDocumentText}" +
                      "\n\nPlease analyze the provided Claim Demographics (Name, Nominee Aadhar) against the raw document text. " +
                      "\nRULES FOR MISMATCHES:" +
                      "\n1. Names DO NOT need to match completely or exactly (e.g., if Name is 'thanmai' but proof says 'krishna thanmai', ignore it. Only check if what they typed exists somewhere in the proofs)." +
                      $"\n2. STRICT MATCH RULE: The submitted 12-digit Nominee Aadhar Number ({claimDetails.NomineeIdNumber}) MUST match CORRECTLY between what the customer provided and what is actually found embedded inside the proofs. " +
                      "\n3. *ONLY* if you find any mismatch between the provided strict fields (Aadhar) and the documents, explicitly append a 'Verification Findings' section at the end of your summary and state that there is a discrepancy. Suggest the officers check the specific mismatch." +
                      "\n4. If the exact Aadhar is verified successfully inside the proofs, DO NOT include a Verification section. Simply end your summary normally. ONLY SHOW INCORRECT DETAILS.";
        }

        var projectId = "astral-comfort-491306-u4";
        var location = "us-central1"; 
        var publisher = "google";
        var model = "gemini-2.5-flash";

        var client = await PredictionServiceClient.CreateAsync();

        var request = new GenerateContentRequest
        {
            Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
            Contents =
            {
                new Content
                {
                    Role = "USER",
                    Parts =
                    {
                        new Part { Text = prompt }
                    }
                }
            }
        };

        var response = await client.GenerateContentAsync(request);
        var textResult = response.Candidates[0].Content.Parts[0].Text;
        return textResult.Replace("**", "");
    }
}
