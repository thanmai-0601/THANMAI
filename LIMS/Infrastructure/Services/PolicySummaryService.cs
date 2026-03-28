using Application.Interfaces.Services;
using Google.Cloud.AIPlatform.V1;

namespace Infrastructure.Services;

public class PolicySummaryService : IPolicySummaryService
{
    private readonly IPolicyService _policyService;

    public PolicySummaryService(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    public async Task<string> GeneratePolicySummaryAsync(int policyId)
    {
        var policyDetails = await _policyService.GetPolicyDetailsAsync(policyId, 0, "Admin");
        
        var prompt = $@"Generate a concise, professional summary of the following life insurance policy and its nominees for the reviewing Agent.
Please ensure the output is a single well-structured paragraph (100-150 words) without markdown, bullet points, or conversional filler.
Clearly include the policy identification, plan details, coverage amount, customer demographics, and the primary nominee's relationship.
Wrap important fields like the Policy Number and Plan Name in double quotes ("" "").

Policy Details:
Policy Number: {policyDetails.PolicyNumber}
Plan Name: {policyDetails.PlanName}
Plan Tenure: {policyDetails.TenureYears} Years
Coverage Amount (Sum Assured): {policyDetails.SumAssured}
Status: {policyDetails.Status}
Submitted Date: {policyDetails.SubmittedAt}

Customer Profile:
Name: {policyDetails.CustomerName}
Age: {policyDetails.CustomerAge}
Occupation: {policyDetails.Occupation}
Income: {policyDetails.AnnualIncome}
Risk Category: {policyDetails.RiskCategory}

Nominees:
";
        // Ensure we explicitly print the Nominees with their Aadhar Numbers so Vertex AI knows what to match
        if (policyDetails.Nominees != null && policyDetails.Nominees.Any())
        {
            foreach (var nom in policyDetails.Nominees)
            {
                prompt += $"- {nom.FullName} (Relationship: {nom.Relationship}, Aadhar ID: {nom.IdNumber})\n";
            }
        }
        else
        {
            prompt += "- No nominees currently registered.\n";
        }

        // ── Extracted Text Injection ──
        string rawDocumentText = "";
        try
        {
            if (policyDetails.Documents != null && policyDetails.Documents.Any())
            {
                var rootDir = Directory.GetCurrentDirectory();
                foreach (var doc in policyDetails.Documents)
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
                      "\nBelow is the raw text extracted from the customer's uploaded PDF documents." +
                      $"\n{rawDocumentText}" +
                      "\n\nPlease analyze the provided Demographics (Name, Nominee Aadhar, Annual Income) against the raw document text. " +
                      "\nRULES FOR MISMATCHES:" +
                      "\n1. Names DO NOT need to match completely or exactly (e.g., if Name is 'thanmai' but proof says 'krishna thanmai', ignore it. Only check if what they typed exists somewhere in the proofs)." +
                      "\n2. STRICT MATCH RULE: The 12-digit Nominee Aadhar Number and the Customer's Annual Income Amount MUST match CORRECTLY between what the customer provided and what is actually found embedded inside the proofs. " +
                      "\n3. *ONLY* if you find any mismatch between the provided strict fields (Aadhar, Income) and the documents, explicitly append a 'Verification Findings' section at the end of your summary and explicitly state that there is a discrepancy and suggest the agent checks the specific mismatch." +
                      "\n4. If the exact Aadhar and Income are verified successfully inside the proofs, DO NOT include a Verification section. Simply end your summary normally. ONLY SHOW INCORRECT DETAILS.";
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
