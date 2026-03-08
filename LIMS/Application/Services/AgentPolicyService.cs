using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AgentPolicyService : IAgentPolicyService
{
    private readonly IPolicyRepository _policyRepo;
    private readonly IPlanRepository _planRepo;
    private readonly INomineeRepository _nomineeRepo;
    private readonly ICommissionRepository _commissionRepo;
    private readonly IPremiumCalculationService _premiumCalc;
    private readonly IInvoiceService _invoiceService;

    public AgentPolicyService(
        IPolicyRepository policyRepo,
        IPlanRepository planRepo,
        INomineeRepository nomineeRepo,
        ICommissionRepository commissionRepo,
        IPremiumCalculationService premiumCalc,
        IInvoiceService invoiceService)
    {
        _policyRepo = policyRepo;
        _planRepo = planRepo;
        _nomineeRepo = nomineeRepo;
        _commissionRepo = commissionRepo;
        _premiumCalc = premiumCalc;
        _invoiceService = invoiceService;
    }

    // ── Step 1: Agent fills eligibility details & calculates premium ──────

    // ── Agent: Assign risk + calculate premium (customer already sent basic details) ─

    public async Task<PremiumCalculationResultDto> CalculatePremiumAsync(
        int policyId, int agentId, AgentPremiumCalculationDto dto)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        EnsureAgentOwnsPolicy(policy, agentId);
        EnsureStatus(policy, PolicyStatus.Submitted,
            "Premium can only be calculated for Submitted policies.");

        var plan = policy.InsurancePlan;

        return _premiumCalc.Calculate(
            plan, policy.SumAssured, policy.TenureYears, dto.RiskCategory);
    }

private static List<string> GetPlanBenefits(
    Domain.Entities.InsurancePlan plan, string riskCategory)
{
    return new List<string>
    {
        $"Death Benefit: Full sum assured of ₹{plan.MinSumAssured:N0}–₹{plan.MaxSumAssured:N0} paid to nominees",
        $"Maturity Benefit: Sum assured returned on policy completion",
        $"Risk Category Assigned: {riskCategory}",
        $"Commission: {plan.CommissionPercentage}% of annual premium",
        "Tax Benefit: Premium eligible for deduction under Section 80C",
        "Grace Period: 30 days after due date before policy lapses"
    };
}

    // ── Step 2: Customer submits nominees ─────────────────────────────────

    public async Task<List<NomineeResponseDto>> SubmitNomineesAsync(
        int policyId, int customerId, SubmitNomineesDto dto)
    {
        var policy = await _policyRepo.GetByIdAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        EnsureCustomerOwnsPolicy(policy, customerId);

        // Policy must be Approved before nominees can be submitted
        if (policy.Status != PolicyStatus.Approved &&
            policy.Status != PolicyStatus.UnderReview &&
            policy.Status != PolicyStatus.Submitted)
            throw new InvalidOperationException(
                "Nominees can only be submitted while policy is accepted but pending customer details.");

        // All allocation percentages must add up to exactly 100
        var totalAllocation = dto.Nominees.Sum(n => n.AllocationPercentage);
        if (totalAllocation != 100)
            throw new InvalidOperationException(
                $"Nominee allocation percentages must total 100%. " +
                $"Current total: {totalAllocation}%.");

        // Clear existing nominees and re-add (allows resubmission)
        await _nomineeRepo.DeleteByPolicyIdAsync(policyId);

        var nominees = dto.Nominees.Select(n => new Nominee
        {
            PolicyId = policyId,
            FullName = n.FullName,
            Relationship = n.Relationship,
            Age = n.Age,
            ContactNumber = n.ContactNumber,
            AllocationPercentage = n.AllocationPercentage
        }).ToList();

        await _nomineeRepo.AddRangeAsync(nominees);
        
        await CheckAndUpdatePolicyActivationAsync(policyId);

        return nominees.Select(MapNomineeToDto).ToList();
    }

    // ── Step 3: Customer uploads document ────────────────────────────────

    public async Task<DocumentResponseDto> UploadDocumentAsync(
        int policyId, int customerId, string documentType,
        string fileName, string filePath)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        EnsureCustomerOwnsPolicy(policy, customerId);

        if (policy.Status != PolicyStatus.Approved &&
            policy.Status != PolicyStatus.UnderReview &&
            policy.Status != PolicyStatus.Submitted &&
            policy.Status != PolicyStatus.Draft)
            throw new InvalidOperationException(
                "Documents can only be uploaded while policy is pending or under evaluation.");

        // Find and remove existing document of the same type (replace logic)
        var existing = policy.Documents.FirstOrDefault(d => d.DocumentType == documentType);
        if (existing != null)
        {
            policy.Documents.Remove(existing);
            // Optionally: delete old file here if desired, but for now we focus on DB sync
        }

        var document = new Document
        {
            PolicyId = policyId,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = filePath,
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTime.UtcNow
        };

        await _policyRepo.AddDocumentAsync(document);
        
        // Final sanity check for policy activation
        await CheckAndUpdatePolicyActivationAsync(policyId);

        return new DocumentResponseDto
        {
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FilePath = document.FilePath,
            Status = document.Status.ToString(),
            UploadedAt = document.UploadedAt
        };
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

    // ── Step 4: Agent approves or rejects ────────────────────────────────

    public async Task<PolicyResponseDto> MakeDecisionAsync(
        int policyId, int agentId, PolicyDecisionDto dto)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        EnsureAgentOwnsPolicy(policy, agentId);
        EnsureStatus(policy, PolicyStatus.Submitted,
            "Decision can only be made on pending policies.");

        if (dto.IsApproved)
        {
            // ── APPROVE ──────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(dto.RiskCategory))
                throw new InvalidOperationException("Risk category is required when approving a policy.");
                
            var calcResult = _premiumCalc.Calculate(
                policy.InsurancePlan, policy.SumAssured, policy.TenureYears, dto.RiskCategory);
                
            policy.RiskCategory = dto.RiskCategory;
            policy.PremiumAmount = calcResult.AnnualPremium;
            policy.AgentRemarks = dto.AgentRemarks;

            // Mark as Approved
            policy.Status = PolicyStatus.Approved;
            policy.ApprovedAt = DateTime.UtcNow;

            await _policyRepo.UpdateAsync(policy);

            // Calculate and record commission for the agent
            await RecordCommissionAsync(policy);
            // ← Auto-generate full invoice schedule on approval
            // Default frequency is Annual — customer can choose later
            await _invoiceService.GenerateScheduleAsync(policy.Id, PaymentFrequency.Annual);

        }
        else
        {
            // ── REJECT ───────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new InvalidOperationException(
                    "Rejection reason is required when rejecting a policy.");

            policy.Status = PolicyStatus.Rejected;
            policy.RejectedAt = DateTime.UtcNow;
            policy.RejectionReason = dto.RejectionReason;

            await _policyRepo.UpdateAsync(policy);
        }

        return MapPolicyToDto(policy);
    }

    // ── Get Nominees ──────────────────────────────────────────────────────

    public async Task<List<NomineeResponseDto>> GetNomineesAsync(int policyId)
    {
        var nominees = await _nomineeRepo.GetByPolicyIdAsync(policyId);
        return nominees.Select(MapNomineeToDto).ToList();
    }

    // ── Private Helpers ───────────────────────────────────────────────────

    private static void EnsureAgentOwnsPolicy(Policy policy, int agentId)
    {
        if (policy.AgentId != agentId)
            throw new UnauthorizedAccessException(
                "You are not assigned to this policy.");
    }

    private static void EnsureCustomerOwnsPolicy(Policy policy, int customerId)
    {
        if (policy.CustomerId != customerId)
            throw new UnauthorizedAccessException(
                "You do not have access to this policy.");
    }

    private static void EnsureStatus(Policy policy, PolicyStatus expected, string message)
    {
        if (policy.Status != expected)
            throw new InvalidOperationException(message);
    }

    private async Task RecordCommissionAsync(Policy policy)
    {
        // Slab system: 0-10 -> 5%, 11-20 -> 8%, 21+ -> 12%
        // (Includes the newly approved policy, so if this is their 1st, count is 1)
        var agentPolicyCount = await _policyRepo.GetActiveCountByAgentAsync(policy.AgentId!.Value) + 1;
        
        decimal slabPercentage = agentPolicyCount switch
        {
            <= 10 => 5m,
            <= 20 => 8m,
            _ => 12m
        };

        var commissionAmount = Math.Round(policy.PremiumAmount!.Value * slabPercentage / 100, 2);

        var commission = new Commission
        {
            PolicyId = policy.Id,
            AgentId = policy.AgentId!.Value,
            PremiumAmount = policy.PremiumAmount.Value,
            CommissionPercentage = slabPercentage,
            CommissionAmount = commissionAmount,
            Status = CommissionStatus.Pending
        };

        await _commissionRepo.CreateAsync(commission);
    }

    private static NomineeResponseDto MapNomineeToDto(Nominee n) => new()
    {
        NomineeId = n.Id,
        FullName = n.FullName,
        Relationship = n.Relationship,
        Age = n.Age,
        ContactNumber = n.ContactNumber,
        AllocationPercentage = n.AllocationPercentage
    };

    private static PolicyResponseDto MapPolicyToDto(Policy p) => new()
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
}