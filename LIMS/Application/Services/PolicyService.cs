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

    public PolicyService(
        IPolicyRepository policyRepo,
        IPlanRepository planRepo,
        IAgentAssignmentService agentAssignment,
        INotificationService notificationService)
    {
        _policyRepo = policyRepo;
        _planRepo = planRepo;
        _agentAssignment = agentAssignment;
        _notificationService = notificationService;
    }

    // ── Customer: Submit Policy Request ───────────────────────────────────
    public async Task<PolicyResponseDto> RequestPolicyAsync(
    int customerId, RequestPolicyDto dto)
    {
        // 1. Validate plan
        var plan = await _planRepo.GetByIdAsync(dto.InsurancePlanId)
            ?? throw new KeyNotFoundException("Insurance plan not found.");

        if (!plan.IsActive)
            throw new InvalidOperationException("This plan is no longer available.");

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

        // 4. Validate age eligibility upfront
        if (dto.CustomerAge < plan.MinEntryAge || dto.CustomerAge > plan.MaxEntryAge)
            throw new InvalidOperationException(
                $"Your age {dto.CustomerAge} is not eligible for this plan. " +
                $"Allowed: {plan.MinEntryAge}–{plan.MaxEntryAge} years.");

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
            CustomerAge = dto.CustomerAge,
            AnnualIncome = dto.AnnualIncome,
            Occupation = dto.Occupation,
            CustomerAddress = dto.Address,
            SelectedRiders = dto.SelectedRiders,

            SubmittedAt = DateTime.UtcNow,
            AgentAssignedAt = DateTime.UtcNow
        };

        await _policyRepo.CreateAsync(policy);

        await _notificationService.CreateNotificationAsync(customerId, $"Your policy request '{policyNumber}' has been submitted successfully and assigned to an agent.");
        if (agentId > 0)
        {
            await _notificationService.CreateNotificationAsync(agentId, $"A new policy request '{policyNumber}' has been assigned to you.");
        }

        var created = await _policyRepo.GetByIdWithDetailsAsync(policy.Id);
        return MapToDto(created!);
    }


    // ── Customer: View My Policies ─────────────────────────────────────────

    public async Task<List<PolicyResponseDto>> GetMyPoliciesAsync(int customerId)
    {
        var policies = await _policyRepo.GetByCustomerIdAsync(customerId);
        return policies.Select(MapToDto).ToList();
    }

    // ── Agent: View Assigned Policies ──────────────────────────────────────

    public async Task<List<PolicyResponseDto>> GetAgentPoliciesAsync(int agentId)
    {
        var policies = await _policyRepo.GetByAgentIdAsync(agentId);
        return policies.Select(MapToDto).ToList();
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

        return MapToDto(policy);
    }

    // ── Admin: View All Policies ───────────────────────────────────────────

    public async Task<List<PolicyResponseDto>> GetAllPoliciesAsync(PolicyStatus? status = null)
    {
        var policies = await _policyRepo.GetAllAsync(status);
        return policies.Select(MapToDto).ToList();
    }

    // ── Private Mapper ─────────────────────────────────────────────────────

    private static PolicyResponseDto MapToDto(Policy p) => new()
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
        SelectedRiders = p.SelectedRiders,
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
            AllocationPercentage = n.AllocationPercentage
        }).ToList() ?? new List<NomineeResponseDto>(),
        Documents = p.Documents?.Select(d => new DocumentResponseDto 
        {
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FilePath = d.FilePath,
            Status = d.Status.ToString(),
            UploadedAt = d.UploadedAt
        }).ToList() ?? new List<DocumentResponseDto>(),
        HasSettledClaim = p.Claims?.Any(c => c.Status == ClaimStatus.Settled) ?? false
    };

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

        // We do *not* clear existing nominees natively right now, we are just appending new ones 
        // per the user workflow in adding individually from the UI instead of all at once.
        var newNominees = dto.Nominees.Select(n => new Nominee
        {
            PolicyId = policyId,
            FullName = n.FullName,
            Relationship = n.Relationship,
            Age = n.Age,
            ContactNumber = n.ContactNumber ?? string.Empty,
            AllocationPercentage = n.AllocationPercentage
        }).ToList();

        // Clear existing nominees before adding the new one (edit mode)
        await _policyRepo.RemoveNomineesAsync(policyId);

        foreach (var nominee in newNominees)
        {
            await _policyRepo.AddNomineeAsync(nominee);
        }

        await CheckAndUpdatePolicyActivationAsync(policyId);

        return new { Message = "Nominees added successfully." };
    }

    private async Task CheckAndUpdatePolicyActivationAsync(int policyId)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId);
        if (policy == null || policy.Status != PolicyStatus.Approved) return;

        bool hasAddressProof = policy.Documents.Any(d => d.DocumentType == "Address Proof");
        bool hasIncomeProof = policy.Documents.Any(d => d.DocumentType == "Income Proof");
        bool hasNomineeId = policy.Documents.Any(d => d.DocumentType == "Nominee ID Proof");
        
        var totalAllocation = policy.Nominees.Sum(n => n.AllocationPercentage);
        
        if (hasAddressProof && hasIncomeProof && hasNomineeId && totalAllocation == 100)
        {
            policy.Status = PolicyStatus.DocumentsSubmitted;
            await _policyRepo.UpdateAsync(policy);
        }
    }
}