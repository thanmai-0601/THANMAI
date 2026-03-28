using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model = "llama-3.3-70b-versatile";
        private readonly IDashboardService _dashboardService;
        private readonly IUserRepository _userRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IClaimRepository _claimRepository;

        public ChatService(
            HttpClient httpClient,
            IConfiguration config,
            IDashboardService dashboardService,
            IUserRepository userRepository,
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository)
        {
            _httpClient = httpClient;
            _apiKey = config["Groq:ApiKey"] ?? ""; // Hardcoded key removed for security; ensure it is set in appsettings.json or environment variables.
            _dashboardService = dashboardService;
            _userRepository = userRepository;
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
        }

        public async Task<ChatResponseDto> GetChatResponseAsync(string message, string role, int userId)
        {
            // 1. Keyword-based Backend Validation (Security)
            if (!IsQueryAllowed(message, role))
            {
                return new ChatResponseDto 
                { 
                    Response = $"I can only assist with {role}-related queries. Please stay within your designated scope.",
                    Role = role
                };
            }

            // 2. Fetch real data from the database based on role
            string contextData = await GetContextDataAsync(role, userId);

            // 3. Role-Based System Prompt with real data
            string systemPrompt = GetSystemPrompt(role, contextData);

            // 4. Prepare Groq API Request
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = message }
                },
                temperature = 0.7,
                max_tokens = 1024
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API Error: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string aiResponse = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            return new ChatResponseDto
            {
                Response = aiResponse,
                Role = role
            };
        }

        private async Task<string> GetContextDataAsync(string role, int userId)
        {
            try
            {
                switch (role)
                {
                    case "Admin":
                        return await GetAdminContextAsync(userId);
                    case "Customer":
                        return await GetCustomerContextAsync(userId);
                    case "Agent":
                        return await GetAgentContextAsync(userId);
                    case "ClaimsOfficer":
                        return await GetClaimsOfficerContextAsync(userId);
                    default:
                        return "No data available for your role.";
                }
            }
            catch (Exception ex)
            {
                return $"Unable to fetch live data: {ex.Message}";
            }
        }

        private async Task<string> GetAdminContextAsync(int userId)
        {
            var adminData = await _dashboardService.GetAdminDashboardAsync();
            var allCustomers = await _userRepository.GetAllAsync(UserRole.Customer);
            var allAgents = await _userRepository.GetAllAsync(UserRole.Agent);
            var allOfficers = await _userRepository.GetAllAsync(UserRole.ClaimsOfficer);
            var allPolicies = await _policyRepository.GetAllAsync();
            var allClaims = await _claimRepository.GetAllAsync();
            var personalPolicies = await _policyRepository.GetByCustomerIdAsync(userId); // Fetch Admin's personal policies

            var sb = new StringBuilder();

            sb.AppendLine("\nYOUR PERSONAL RECORDS (AS AN ADMIN USER):");
            if (personalPolicies.Count > 0)
            {
                foreach (var p in personalPolicies)
                    sb.AppendLine($"- Policy {p.PolicyNumber}, Plan: {p.InsurancePlan?.PlanName}, Status: {p.Status}");
            }
            else
            {
                sb.AppendLine("- You do not have any personal insurance policies yourself.");
            }

            sb.AppendLine("\nSYSTEM-WIDE LIVE PLATFORM DATA:");
            sb.AppendLine($"- Total Policies: {adminData.TotalPolicies} (Active: {adminData.ActivePolicies}, Submitted: {adminData.SubmittedPolicies}, Under Review: {adminData.UnderReviewPolicies}, Rejected: {adminData.RejectedPolicies}, Lapsed: {adminData.LapsedPolicies}, Settled: {adminData.SettledPolicies})");
            sb.AppendLine($"- Total Claims: {adminData.TotalClaims} (Submitted: {adminData.SubmittedClaims}, Under Review: {adminData.UnderReviewClaims}, Settled: {adminData.SettledClaims}, Rejected: {adminData.RejectedClaims})");
            sb.AppendLine($"- Total Premium Collected: Rs.{adminData.TotalPremiumCollected:N2}");
            sb.AppendLine($"- Total Commission Paid: Rs.{adminData.TotalCommissionPaid:N2}");
            sb.AppendLine($"- Total Settled Claims Amount: Rs.{adminData.TotalSettledAmount:N2}");

            sb.AppendLine($"\nCUSTOMERS ({allCustomers.Count}):");
            foreach (var c in allCustomers.Where(c => !c.IsDeleted))
                sb.AppendLine($"  - {c.FullName} (Email: {c.Email}, Phone: {c.PhoneNumber}, Active: {c.IsActive})");

            sb.AppendLine($"\nAGENTS ({allAgents.Count}):");
            foreach (var a in allAgents.Where(a => !a.IsDeleted))
                sb.AppendLine($"  - {a.FullName} (Email: {a.Email}, Active: {a.IsActive})");

            sb.AppendLine($"\nCLAIMS OFFICERS ({allOfficers.Count}):");
            foreach (var o in allOfficers.Where(o => !o.IsDeleted))
                sb.AppendLine($"  - {o.FullName} (Email: {o.Email}, Active: {o.IsActive})");

            sb.AppendLine($"\nALL POLICIES ({allPolicies.Count}):");
            foreach (var p in allPolicies.Take(20))
                sb.AppendLine($"  - {p.PolicyNumber}, Status: {p.Status}, Customer: {p.Customer?.FullName ?? "N/A"}, Agent: {p.Agent?.FullName ?? "Not Assigned"}, Plan: {p.InsurancePlan?.PlanName ?? "N/A"}, Sum Assured: Rs.{p.SumAssured:N2}, Premium: Rs.{p.PremiumAmount?.ToString("N2") ?? "Pending"}");

            sb.AppendLine($"\nALL CLAIMS ({allClaims.Count}):");
            foreach (var cl in allClaims.Take(20))
                sb.AppendLine($"  - {cl.ClaimNumber}, Status: {cl.Status}, Type: {cl.Type}, Amount: Rs.{cl.ClaimAmount:N2}, Policy: {cl.Policy?.PolicyNumber ?? "N/A"}");

            return sb.ToString();
        }

        private async Task<string> GetCustomerContextAsync(int userId)
        {
            var custData = await _dashboardService.GetCustomerDashboardAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);
            var policies = await _policyRepository.GetByCustomerIdAsync(userId);
            var claims = await _claimRepository.GetByCustomerIdAsync(userId);

            var sb = new StringBuilder();
            sb.AppendLine($"\nYOUR ACCOUNT: {user?.FullName ?? "Customer"}");
            sb.AppendLine($"- Email: {user?.Email}, Phone: {user?.PhoneNumber}");
            sb.AppendLine($"- Total Policies: {custData.TotalPolicies} (Active: {custData.ActivePolicies}, Pending: {custData.PendingPolicies}, Rejected: {custData.RejectedPolicies})");
            sb.AppendLine($"- Total Claims: {custData.TotalClaims} (Open: {custData.OpenClaims}, Settled: {custData.SettledClaims})");
            sb.AppendLine($"- Total Paid: Rs.{custData.TotalPaidAmount:N2}, Outstanding: Rs.{custData.TotalOutstandingAmount:N2}");
            sb.AppendLine($"- Overdue Invoices: {custData.OverdueInvoices}, Upcoming: {custData.UpcomingInvoices}");

            sb.AppendLine($"\nYOUR POLICIES:");
            foreach (var p in policies)
                sb.AppendLine($"  - {p.PolicyNumber}, Plan: {p.InsurancePlan?.PlanName ?? "N/A"}, Status: {p.Status}, Sum Assured: Rs.{p.SumAssured:N2}, Premium: Rs.{p.PremiumAmount?.ToString("N2") ?? "Pending"}, Tenure: {p.TenureYears} years");

            sb.AppendLine($"\nYOUR CLAIMS:");
            foreach (var cl in claims)
                sb.AppendLine($"  - {cl.ClaimNumber}, Type: {cl.Type}, Status: {cl.Status}, Amount: Rs.{cl.ClaimAmount:N2}");

            return sb.ToString();
        }

        private async Task<string> GetAgentContextAsync(int userId)
        {
            var agentData = await _dashboardService.GetAgentDashboardAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);
            var policies = await _policyRepository.GetByAgentIdAsync(userId);

            var sb = new StringBuilder();
            sb.AppendLine($"\nYOUR AGENT PROFILE: {user?.FullName ?? "Agent"}");
            sb.AppendLine($"- Total Assigned Policies: {agentData.TotalAssignedPolicies} (Active: {agentData.ActivePolicies}, Submitted: {agentData.SubmittedPolicies}, Under Review: {agentData.UnderReviewPolicies}, Rejected: {agentData.RejectedPolicies})");
            sb.AppendLine($"- Commission Earned: Rs.{agentData.TotalCommissionEarned:N2}, Pending: Rs.{agentData.PendingCommission:N2}");
            sb.AppendLine($"- This Month: Rs.{agentData.ThisMonthCommission:N2}, Last Month: Rs.{agentData.LastMonthCommission:N2}");

            sb.AppendLine($"\nYOUR ASSIGNED POLICIES:");
            foreach (var p in policies.Take(20))
                sb.AppendLine($"  - {p.PolicyNumber}, Customer: {p.Customer?.FullName ?? "N/A"}, Status: {p.Status}, Plan: {p.InsurancePlan?.PlanName ?? "N/A"}, Sum Assured: Rs.{p.SumAssured:N2}, Premium: Rs.{p.PremiumAmount?.ToString("N2") ?? "Pending"}");

            return sb.ToString();
        }

        private async Task<string> GetClaimsOfficerContextAsync(int userId)
        {
            var coData = await _dashboardService.GetClaimsOfficerDashboardAsync(userId);
            var user = await _userRepository.GetByIdAsync(userId);
            var claims = await _claimRepository.GetByOfficerIdAsync(userId);

            var sb = new StringBuilder();
            sb.AppendLine($"\nYOUR CLAIMS OFFICER PROFILE: {user?.FullName ?? "Officer"}");
            sb.AppendLine($"- Total Assigned Claims: {coData.TotalAssignedClaims} (Submitted: {coData.SubmittedClaims}, Under Review: {coData.UnderReviewClaims}, Settled: {coData.SettledClaims}, Rejected: {coData.RejectedClaims})");
            sb.AppendLine($"- Total Settled Amount: Rs.{coData.TotalSettledAmount:N2}");
            sb.AppendLine($"- This Month Settled: Rs.{coData.ThisMonthSettledAmount:N2}");

            sb.AppendLine($"\nYOUR ASSIGNED CLAIMS:");
            foreach (var cl in claims.Take(20))
                sb.AppendLine($"  - {cl.ClaimNumber}, Type: {cl.Type}, Status: {cl.Status}, Amount: Rs.{cl.ClaimAmount:N2}, Policy: {cl.Policy?.PolicyNumber ?? "N/A"}, Customer: {cl.Policy?.Customer?.FullName ?? "N/A"}");

            return sb.ToString();
        }

        private string GetSystemPrompt(string role, string contextData)
        {
            string baseInstruction = @"You are NexaLife AI Assistant for a Life Insurance Management System.

STRICT RESPONSE RULES:
1. Answer ONLY based on the LIVE DATA provided below. Never make up data.
2. Give direct, specific answers using actual names, numbers, and policy numbers from the data.
3. Format your response as clean, numbered points or short paragraphs. Keep it natural and human-like.
4. NEVER use markdown formatting. No **, no ##, no *italics*, no bullet symbols like •.
5. Use simple dashes (-) or numbers (1, 2, 3) for listing items.
6. Keep answers concise and to the point. No unnecessary filler text.
7. When listing people or items, present them clearly with their relevant details on each line.
8. Use Rs. for currency amounts.
9. If asked something not in the data, simply say 'That information is not available in the current data.'";

            return role switch
            {
                "Admin" => $@"{baseInstruction}

You are assisting the ADMIN. 
- When they say 'my policies/claims' or 'policies I took', they mean their PERSOANL records. 
- When they ask about 'the system', 'the platform', 'all policies', or 'total count', they mean the overall platform data.
- Always distinguish between their own personal data (if any) and the system-wide statistics.
{contextData}",

                "Agent" => $@"{baseInstruction}

You are assisting an AGENT. Help with their assigned policies, customers, and commissions. Give specific policy numbers and customer names.
{contextData}",

                "Customer" => $@"{baseInstruction}

You are assisting a CUSTOMER about their own account. Give specific details about their policies, claims, and payments. Be helpful and supportive.
{contextData}",

                "ClaimsOfficer" => $@"{baseInstruction}

You are assisting a CLAIMS OFFICER. Help with their assigned claims, giving specific claim numbers, customer names, and amounts.
{contextData}",

                _ => $"{baseInstruction}\nNo specific role context available."
            };
        }

        private bool IsQueryAllowed(string message, string role)
        {
            message = message.ToLower();
            
            var forbidden = new[] { "ignore your instructions", "system prompt", "developer mode", "override" };
            if (forbidden.Any(f => message.Contains(f))) return false;

            return role switch
            {
                "Admin" => true,
                "Customer" => !message.Contains("commission") && !message.Contains("agent details") && !message.Contains("system settings"),
                "Agent" => !message.Contains("admin password") && !message.Contains("delete user") && !message.Contains("global revenue"),
                "ClaimsOfficer" => !message.Contains("marketing") && !message.Contains("commission") && !message.Contains("create policy"),
                _ => false
            };
        }
    }
}
