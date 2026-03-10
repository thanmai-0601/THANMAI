using System.Text.Json;
using Application.DTOs.Endorsement;
using Application.DTOs.Policy;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EndorsementService : IEndorsementService
{
    private readonly IEndorsementRepository _endorsementRepo;
    private readonly IPolicyRepository _policyRepo;
    private readonly INomineeRepository _nomineeRepo;

    public EndorsementService(
        IEndorsementRepository endorsementRepo,
        IPolicyRepository policyRepo,
        INomineeRepository nomineeRepo)
    {
        _endorsementRepo = endorsementRepo;
        _policyRepo = policyRepo;
        _nomineeRepo = nomineeRepo;
    }

    // ── Address Change ─────────────────────────────────────────────────────

    public async Task<EndorsementResponseDto> RequestAddressChangeAsync(
        int customerId, RequestAddressChangeDto dto)
    {
        var policy = await ValidatePolicyForEndorsement(dto.PolicyId, customerId);

        var endorsement = new PolicyEndorsement
        {
            PolicyId = dto.PolicyId,
            RequestedByCustomerId = customerId,
            Type = EndorsementType.AddressChange,
            Status = EndorsementStatus.Requested,
            // Store new value as JSON
            ChangeRequestJson = JsonSerializer.Serialize(
                new { NewAddress = dto.NewAddress }),
            // Store old value for audit trail
            OldValueJson = JsonSerializer.Serialize(
                new { oldAddress = policy.CustomerAddress ?? "Not set" }),
            RequestedAt = DateTime.UtcNow
        };

        await _endorsementRepo.CreateAsync(endorsement);
        var created = await _endorsementRepo.GetByIdWithDetailsAsync(endorsement.Id);
        return MapToDto(created!);
    }

    // ── Nominee Change ─────────────────────────────────────────────────────

    public async Task<EndorsementResponseDto> RequestNomineeChangeAsync(
        int customerId, RequestNomineeChangeDto dto)
    {
        await ValidatePolicyForEndorsement(dto.PolicyId, customerId);

        // Get current nominees for old value snapshot
        var currentNominees = await _nomineeRepo.GetByPolicyIdAsync(dto.PolicyId);
        var oldSnapshot = currentNominees.Select(n => new
        {
            n.FullName,
            n.Relationship,
            n.Age,
            AllocationPercentage = 100
        });

        var endorsement = new PolicyEndorsement
        {
            PolicyId = dto.PolicyId,
            RequestedByCustomerId = customerId,
            Type = EndorsementType.NomineeChange,
            Status = EndorsementStatus.Requested,
            ChangeRequestJson = JsonSerializer.Serialize(dto.NewNominee),
            OldValueJson = JsonSerializer.Serialize(oldSnapshot),
            RequestedAt = DateTime.UtcNow
        };

        await _endorsementRepo.CreateAsync(endorsement);
        var created = await _endorsementRepo.GetByIdWithDetailsAsync(endorsement.Id);
        return MapToDto(created!);
    }

    // ── Sum Assured Increase ───────────────────────────────────────────────

    public async Task<EndorsementResponseDto> RequestSumAssuredIncreaseAsync(
        int customerId, RequestSumAssuredIncreaseDto dto)
    {
        var policy = await ValidatePolicyForEndorsement(dto.PolicyId, customerId);

        // New sum assured must be higher than current
        if (dto.NewSumAssured <= policy.SumAssured)
            throw new InvalidOperationException(
                $"New sum assured ₹{dto.NewSumAssured:N0} must be greater than " +
                $"current ₹{policy.SumAssured:N0}.");

        // Cannot exceed plan maximum
        var plan = policy.InsurancePlan;
        if (dto.NewSumAssured > plan.MaxSumAssured)
            throw new InvalidOperationException(
                $"New sum assured ₹{dto.NewSumAssured:N0} exceeds plan maximum " +
                $"of ₹{plan.MaxSumAssured:N0}.");

        var endorsement = new PolicyEndorsement
        {
            PolicyId = dto.PolicyId,
            RequestedByCustomerId = customerId,
            Type = EndorsementType.SumAssuredIncrease,
            Status = EndorsementStatus.Requested,
            ChangeRequestJson = JsonSerializer.Serialize(
                new { NewSumAssured = dto.NewSumAssured }),
            OldValueJson = JsonSerializer.Serialize(
                new { oldSumAssured = policy.SumAssured }),
            RequestedAt = DateTime.UtcNow
        };

        await _endorsementRepo.CreateAsync(endorsement);
        var created = await _endorsementRepo.GetByIdWithDetailsAsync(endorsement.Id);
        return MapToDto(created!);
    }

    // ── Customer: View My Endorsements ────────────────────────────────────

    public async Task<List<EndorsementResponseDto>> GetMyEndorsementsAsync(
        int customerId)
    {
        var endorsements = await _endorsementRepo.GetByCustomerIdAsync(customerId);
        return endorsements.Select(MapToDto).ToList();
    }

    // ── Agent: View Pending Endorsements ──────────────────────────────────

    public async Task<List<EndorsementResponseDto>> GetPendingEndorsementsAsync(
    int agentId)
    {
        var endorsements = await _endorsementRepo.GetPendingByAgentAsync(agentId);
        return endorsements.Select(e => MapToDto(e)).ToList();  // ← explicit lambda
    }

    // ── Agent: Approve or Reject ───────────────────────────────────────────

    public async Task<EndorsementResponseDto> MakeDecisionAsync(
        int endorsementId, int agentId, EndorsementDecisionDto dto)
    {
        var endorsement = await _endorsementRepo.GetByIdWithDetailsAsync(endorsementId)
            ?? throw new KeyNotFoundException("Endorsement not found.");

        // Agent must be assigned to the policy
        if (endorsement.Policy.AgentId != agentId)
            throw new UnauthorizedAccessException(
                "You are not the assigned agent for this policy.");

        if (endorsement.Status != EndorsementStatus.Requested &&
            endorsement.Status != EndorsementStatus.UnderReview)
            throw new InvalidOperationException(
                "This endorsement has already been processed.");

        endorsement.ReviewedByAgentId = agentId;
        endorsement.ReviewedAt = DateTime.UtcNow;
        endorsement.AgentRemarks = dto.AgentRemarks;

        if (dto.IsApproved)
        {
            endorsement.Status = EndorsementStatus.Approved;
            endorsement.ApprovedAt = DateTime.UtcNow;

            // Apply the actual change to the policy
            await ApplyEndorsementAsync(endorsement);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new InvalidOperationException(
                    "Rejection reason is required.");

            endorsement.Status = EndorsementStatus.Rejected;
            endorsement.RejectionReason = dto.RejectionReason;
        }

        await _endorsementRepo.UpdateAsync(endorsement);
        return MapToDto(endorsement);
    }

    // ── Shared: Get By Id ─────────────────────────────────────────────────

    public async Task<EndorsementResponseDto> GetByIdAsync(
        int endorsementId, int requestingUserId, string role)
    {
        var endorsement = await _endorsementRepo
            .GetByIdWithDetailsAsync(endorsementId)
            ?? throw new KeyNotFoundException("Endorsement not found.");

        var hasAccess = role switch
        {
            "Admin" => true,
            "Customer" => endorsement.RequestedByCustomerId == requestingUserId,
            "Agent" => endorsement.Policy.AgentId == requestingUserId,
            _ => false
        };

        if (!hasAccess)
            throw new UnauthorizedAccessException(
                "You do not have access to this endorsement.");

        return MapToDto(endorsement);
    }

    public async Task<List<EndorsementResponseDto>> GetByPolicyIdAsync(int policyId)
    {
        var endorsements = await _endorsementRepo.GetByPolicyIdAsync(policyId);
        return endorsements.Select(MapToDto).ToList();
    }

    // ── Admin: All Endorsements ───────────────────────────────────────────

    public async Task<List<EndorsementResponseDto>> GetAllAsync(
        EndorsementStatus? status = null)
    {
        var endorsements = await _endorsementRepo.GetAllAsync(status);
        return endorsements.Select(MapToDto).ToList();
    }

    // ── Private: Apply approved change to policy ──────────────────────────

    private async Task ApplyEndorsementAsync(PolicyEndorsement endorsement)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(endorsement.PolicyId)!
            ?? throw new KeyNotFoundException("Policy not found.");

        switch (endorsement.Type)
        {
            case EndorsementType.AddressChange:
                var addressChange = JsonSerializer.Deserialize<AddressChangePayload>(
                    endorsement.ChangeRequestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                policy.CustomerAddress = addressChange.NewAddress;
                await _policyRepo.UpdateAsync(policy);
                break;

            case EndorsementType.NomineeChange:
                var newNominee = JsonSerializer
                    .Deserialize<AddNomineeDto>(endorsement.ChangeRequestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                // Replace all existing nominees with new ones
                await _nomineeRepo.DeleteByPolicyIdAsync(endorsement.PolicyId);
                var nominee = new Nominee
                {
                    PolicyId = endorsement.PolicyId,
                    FullName = newNominee.FullName,
                    Relationship = newNominee.Relationship,
                    Age = newNominee.Age,
                    ContactNumber = newNominee.ContactNumber,
                    Email = newNominee.Email ?? string.Empty,
                    IdNumber = newNominee.IdNumber ?? string.Empty,
                    AllocationPercentage = 100
                };
                await _nomineeRepo.AddRangeAsync(new List<Nominee> { nominee });
                break;

            case EndorsementType.SumAssuredIncrease:
                var sumChange = JsonSerializer.Deserialize<SumAssuredChangePayload>(
                    endorsement.ChangeRequestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                policy.SumAssured = sumChange.NewSumAssured;
                await _policyRepo.UpdateAsync(policy);
                break;
        }
    }

    // ── Private: Validate policy before creating endorsement ──────────────

    private async Task<Policy> ValidatePolicyForEndorsement(
        int policyId, int customerId)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        if (policy.CustomerId != customerId)
            throw new UnauthorizedAccessException(
                "You can only request endorsements on your own policies.");

        if (policy.Status == PolicyStatus.Rejected || policy.Status == PolicyStatus.Cancelled || policy.Status == PolicyStatus.Settled)
            throw new InvalidOperationException(
                "Endorsements cannot be requested on Rejected, Cancelled, or Settled policies.");

        if (policy.Claims.Any(c => c.Status == ClaimStatus.Settled))
            throw new InvalidOperationException(
                "Endorsements cannot be requested after a claim has been settled.");

        return policy;
    }

    // ── Private: Mapper ───────────────────────────────────────────────────

    private static EndorsementResponseDto MapToDto(PolicyEndorsement e) => new()
    {
        EndorsementId = e.Id,
        PolicyId = e.PolicyId,
        PolicyNumber = e.Policy?.PolicyNumber ?? string.Empty,
        Type = e.Type.ToString(),
        Status = e.Status.ToString(),
        ChangeRequested = e.ChangeRequestJson,
        OldValue = e.OldValueJson,
        CustomerName = e.RequestedByCustomer?.FullName ?? string.Empty,
        AgentName = e.ReviewedByAgent?.FullName,
        AgentRemarks = e.AgentRemarks,
        RejectionReason = e.RejectionReason,
        RequestedAt = e.RequestedAt,
        ReviewedAt = e.ReviewedAt,
        ApprovedAt = e.ApprovedAt
    };

    // Helper records for JSON deserialization
    private record AddressChangePayload(string NewAddress);
    private record SumAssuredChangePayload(decimal NewSumAssured);
}