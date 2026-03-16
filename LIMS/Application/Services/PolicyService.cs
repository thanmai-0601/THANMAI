using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IAgentAssignmentService _agentAssignment;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepo;
    private readonly IClaimRepository _claimRepo;

    public PolicyService(
        IPolicyRepository policyRepo,
        IPlanRepository planRepo,
        IAgentAssignmentService agentAssignment,
        INotificationService notificationService,
        IUserRepository userRepo,
        IClaimRepository claimRepo)
    {
        _policyRepo = policyRepo;
        _planRepo = planRepo;
        _agentAssignment = agentAssignment;
        _notificationService = notificationService;
        _userRepo = userRepo;
        _claimRepo = claimRepo;
    }

    // ── Customer: Submit Policy Request ───────────────────────────────────
    public async Task<PolicyResponseDto> RequestPolicyAsync(
    int customerId, RequestPolicyDto dto)
    {
        // 1. Validate plan
        var plan = await _planRepo.GetByIdAsync(dto.InsurancePlanId)
            ?? throw new KeyNotFoundException("Insurance plan not found.");

        // Check if customer has any settled death claims - if so, they cannot take new policies
        var claims = await _claimRepo.GetByCustomerIdAsync(customerId);
        if (claims.Any(c => c.Type == ClaimType.Death && c.Status == ClaimStatus.Settled))
        {
            throw new InvalidOperationException("New policy requests are not allowed as a death claim has already been settled for this account.");
        }

        if (!plan.IsActive)
            throw new InvalidOperationException("This plan is no longer available.");

        // Calculate Customer Age from DOB
        var user = await _userRepo.GetByIdAsync(customerId) 
            ?? throw new KeyNotFoundException("Customer not found.");
        
        var today = DateTime.UtcNow;
        var customerAge = today.Year - user.DateOfBirth.Year;
        if (user.DateOfBirth.Date > today.AddYears(-customerAge)) customerAge--;

        // 2. Validate sum assured range
        if (dto.SumAssured < plan.MinSumAssured || dto.SumAssured > plan.MaxSumAssured)
            throw new InvalidOperationException(
                $"Sum assured must be between {plan.MinSumAssured:N0} " +
                $"and {plan.MaxSumAssured:N0} for this plan.");

        // 3. Validate tenure
        var validTenures = plan.TenureOptions.Split(',').Select(int.Parse).ToList();
        if (!validTenures.Contains(dto.TenureYears))
            throw new InvalidOperationException(
                $"Invalid tenure. Allowed options: {plan.TenureOptions} years.");

        // 4. Validate age eligibility upfront using calculated age
        if (customerAge < plan.MinEntryAge || customerAge > plan.MaxEntryAge)
            throw new InvalidOperationException("not eligible as age is less");

        // 5. Validate income eligibility upfront
        if (dto.AnnualIncome < plan.MinAnnualIncome)
            throw new InvalidOperationException(
                $"Minimum annual income of ₹{plan.MinAnnualIncome:N0} required for this plan.");

        // 6. Auto-assign agent
        var agentId = await _agentAssignment.AssignAgentAsync();
        var policyNumber = await _policyRepo.GeneratePolicyNumberAsync();

        // 7. Create policy with customer's basic details already filled
        var policy = new Policy
        {
            PolicyNumber = policyNumber,
            Status = PolicyStatus.Submitted,
            CustomerId = customerId,
            InsurancePlanId = dto.InsurancePlanId,
            SumAssured = dto.SumAssured,
            TenureYears = dto.TenureYears,
            AgentId = agentId,

            // ← Customer fills these at enrollment now
            CustomerAge = customerAge,
            AnnualIncome = dto.AnnualIncome,
            Occupation = dto.Occupation,
            CustomerAddress = dto.Address,

            SubmittedAt = DateTime.UtcNow,
            AgentAssignedAt = DateTime.UtcNow
        };

        // 8. Process Nominee upfront
        if (dto.Nominee != null)
        {
            policy.Nominees.Add(new Nominee
            {
                FullName = dto.Nominee.FullName,
                Relationship = dto.Nominee.Relationship,
                Age = dto.Nominee.Age,
                ContactNumber = dto.Nominee.ContactNumber,
                IdNumber = dto.Nominee.IdNumber,
                Email = dto.Nominee.Email,
                AllocationPercentage = 100 // Single nominee gets full
            });
        }

        await _policyRepo.CreateAsync(policy);

        // Now that the policy ID is available, process documents
        if (dto.Documents != null && dto.Documents.Any())
        {
            foreach (var d in dto.Documents)
            {
                var base64Data = d.FileBase64.Split(',').Last();
                var fileBytes = Convert.FromBase64String(base64Data);
                
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", policy.Id.ToString());
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, d.FileName);
                await File.WriteAllBytesAsync(filePath, fileBytes);

                var document = new Document
                {
                    PolicyId = policy.Id,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    FilePath = $"/uploads/{policy.Id}/{d.FileName}",
                    UploadedAt = DateTime.UtcNow,
                    Status = DocumentStatus.Submitted
                };
                await _policyRepo.AddDocumentAsync(document);
            }
        }

        await _notificationService.CreateNotificationAsync(customerId, $"Your policy request '{policyNumber}' has been submitted successfully and assigned to an agent.");
        if (agentId > 0)
        {
            await _notificationService.CreateNotificationAsync(agentId, $"A new policy request '{policyNumber}' has been assigned to you.");
        }

        var created = await _policyRepo.GetByIdWithDetailsAsync(policy.Id);
        return await MapToDtoAsync(created!);
    }


    // ── Customer: View My Policies ─────────────────────────────────────────
    public async Task<List<PolicyResponseDto>> GetMyPoliciesAsync(int customerId)
    {
        var policies = await _policyRepo.GetByCustomerIdAsync(customerId);
        var dtos = new List<PolicyResponseDto>();
        foreach (var p in policies)
        {
            dtos.Add(await MapToDtoAsync(p));
        }
        return dtos;
    }

    // ── Agent: View Assigned Policies ──────────────────────────────────────
    public async Task<List<PolicyResponseDto>> GetAgentPoliciesAsync(int agentId)
    {
        var policies = await _policyRepo.GetByAgentIdAsync(agentId);
        var dtos = new List<PolicyResponseDto>();
        foreach (var p in policies)
        {
            dtos.Add(await MapToDtoAsync(p));
        }
        return dtos;
    }

    // ── Shared: Get Single Policy (role-aware access control) ─────────────

    public async Task<PolicyResponseDto> GetPolicyDetailsAsync(
        int policyId, int requestingUserId, string role)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        // Access control — each role can only see what belongs to them
        var hasAccess = role switch
        {
            "Admin" => true,                                  // Admin sees all
            "Customer" => policy.CustomerId == requestingUserId, // own policies only
            "Agent" => policy.AgentId == requestingUserId,    // assigned policies only
            _ => false
        };

        if (!hasAccess)
            throw new UnauthorizedAccessException(
                "You do not have access to this policy.");

        return await MapToDtoAsync(policy);
    }

    // ── Admin: View All Policies ───────────────────────────────────────────

    public async Task<List<PolicyResponseDto>> GetAllPoliciesAsync(PolicyStatus? status = null)
    {
        var policies = await _policyRepo.GetAllAsync(status);
        var dtos = new List<PolicyResponseDto>();
        foreach (var p in policies)
        {
            dtos.Add(await MapToDtoAsync(p));
        }
        return dtos;
    }

    // ── Private Mapper ─────────────────────────────────────────────────────

    private async Task<PolicyResponseDto> MapToDtoAsync(Policy p)
    {
        var customerClaims = await _claimRepo.GetByCustomerIdAsync(p.CustomerId);
        var hasGlobalSettledDeathClaim = customerClaims.Any(c => c.Type == ClaimType.Death && c.Status == ClaimStatus.Settled);

        return new PolicyResponseDto
        {
            PolicyId = p.Id,
            PolicyNumber = p.PolicyNumber,
            Status = (p.Claims?.Any(c => c.Status == ClaimStatus.Settled) ?? false)
                ? PolicyStatus.Settled.ToString()
                : p.Status.ToString(),
            InsurancePlanId = p.InsurancePlanId,
            PlanName = p.InsurancePlan?.PlanName ?? string.Empty,
            SumAssured = p.SumAssured,
            TenureYears = p.TenureYears,
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.FullName ?? string.Empty,
            CustomerEmail = p.Customer?.Email ?? string.Empty,
            AgentId = p.AgentId,
            AgentName = p.Agent?.FullName,

            AgentEmail = p.Agent?.Email,
            CustomerAge = p.CustomerAge,
            AnnualIncome = p.AnnualIncome,
            Occupation = p.Occupation,
            RiskCategory = p.RiskCategory,
            PremiumAmount = p.PremiumAmount,
            AgentRemarks = p.AgentRemarks,
            RejectionReason = p.RejectionReason,
            CreatedAt = p.CreatedAt,
            SubmittedAt = p.SubmittedAt,
            AgentAssignedAt = p.AgentAssignedAt,
            ApprovedAt = p.ApprovedAt,
            ActiveFrom = p.ActiveFrom,
            ActiveTo = p.ActiveTo,
            Nominees = p.Nominees?.Select(n => new NomineeResponseDto
            {
                NomineeId = n.Id,
                FullName = n.FullName,
                Relationship = n.Relationship,
                Age = n.Age,
                ContactNumber = n.ContactNumber,
                IdNumber = n.IdNumber,
                Email = n.Email
            }).ToList() ?? new List<NomineeResponseDto>(),
            Documents = p.Documents?.Select(d => new DocumentResponseDto
            {
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FilePath = d.FilePath,
                Status = d.Status.ToString(),
                UploadedAt = d.UploadedAt
            }).ToList() ?? new List<DocumentResponseDto>(),
            HasSettledClaim = p.Claims?.Any(c => c.Status == ClaimStatus.Settled) ?? false,
            HasGlobalSettledDeathClaim = hasGlobalSettledDeathClaim
        };
    }

    public async Task<object> UploadDocumentAsync(
    int policyId,
    int customerId,
    string documentType,
    string fileName,
    string fileBase64)
    {
        var policy = await _policyRepo.GetByIdAsync(policyId);

        if (policy == null)
            throw new KeyNotFoundException("Policy not found.");

        if (policy.Status == PolicyStatus.Settled || policy.Status == PolicyStatus.Cancelled || policy.Status == PolicyStatus.Rejected)
            throw new InvalidOperationException($"Cannot upload documents for a policy with status {policy.Status}.");

        if (policy.CustomerId != customerId)
            throw new UnauthorizedAccessException("You cannot upload documents for this policy.");

        // Clean base64 string
        var base64Data = fileBase64.Split(',').Last();
        var fileBytes = Convert.FromBase64String(base64Data);

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", policyId.ToString());
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileName);
        await File.WriteAllBytesAsync(filePath, fileBytes);

        var document = new Document
        {
            PolicyId = policyId,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = $"/uploads/{policyId}/{fileName}",
            UploadedAt = DateTime.UtcNow,
            Status = DocumentStatus.Submitted
        };

        await _policyRepo.AddDocumentAsync(document);

        await CheckAndUpdatePolicyActivationAsync(policyId);

        return new
        {
            Message = "Document uploaded successfully.",
            document.DocumentType,
            document.FileName
        };
    }

    public async Task<object> SubmitNomineesAsync(int policyId, int customerId, SubmitNomineesDto dto)
    {
        var policy = await _policyRepo.GetByIdAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        if (policy.CustomerId != customerId)
            throw new UnauthorizedAccessException(
                "You do not have access to this policy.");

        if (policy.Status == PolicyStatus.Settled || policy.Status == PolicyStatus.Cancelled || policy.Status == PolicyStatus.Rejected)
            throw new InvalidOperationException($"Cannot change nominees for a policy with status {policy.Status}.");

        // Clear existing nominees and re-add (allows resubmission)
        await _policyRepo.RemoveNomineesAsync(policyId);

        var nominee = new Nominee
        {
            PolicyId = policyId,
            FullName = dto.Nominee.FullName,
            Relationship = dto.Nominee.Relationship,
            Age = dto.Nominee.Age,
            ContactNumber = dto.Nominee.ContactNumber ?? string.Empty,
            IdNumber = dto.Nominee.IdNumber ?? string.Empty,
            Email = dto.Nominee.Email ?? string.Empty,
            AllocationPercentage = 100
        };

        await _policyRepo.AddNomineeAsync(nominee);


        await CheckAndUpdatePolicyActivationAsync(policyId);

        return new { Message = "Nominees added successfully." };
    }

    private async Task CheckAndUpdatePolicyActivationAsync(int policyId)
    {
        // Removed: We no longer transition to DocumentsSubmitted automatically.
        // Policy remains 'Approved' until paid.
        await Task.CompletedTask;
    }
}