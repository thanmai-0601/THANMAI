using Application.DTOs.Claim;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepo;
    private readonly IPolicyRepository _policyRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IClaimsOfficerAssignmentService _officerAssignment;
    private readonly INotificationService _notificationService;

    public ClaimService(
        IClaimRepository claimRepo,
        IPolicyRepository policyRepo,
        IInvoiceRepository invoiceRepo,
        IClaimsOfficerAssignmentService officerAssignment,
        INotificationService notificationService)
    {
        _claimRepo = claimRepo;
        _policyRepo = policyRepo;
        _invoiceRepo = invoiceRepo;
        _officerAssignment = officerAssignment;
        _notificationService = notificationService;
    }

    // ── Customer: Raise a Claim ────────────────────────────────────────────

    public async Task<ClaimResponseDto> RaiseClaimAsync(RaiseClaimDto dto)
    {
        var policy = await _policyRepo.GetByPolicyNumberWithDetailsAsync(dto.PolicyNumber)
            ?? throw new KeyNotFoundException("Policy not found.");

        // Policy must be Active — no claims on Suspended or Lapsed
        if (policy.Status != PolicyStatus.Active)
            throw new InvalidOperationException(
                $"Claims cannot be raised on a policy with status '{policy.Status}'. " +
                "Policy must be Active.");

        // Check no existing open claim on this policy
        var existingClaims = await _claimRepo.GetByCustomerIdAsync(policy.CustomerId);
        var hasOpenClaim = existingClaims.Any(c =>
            c.PolicyId == policy.Id &&
            c.Status != ClaimStatus.Settled &&
            c.Status != ClaimStatus.Rejected);

        if (hasOpenClaim)
            throw new InvalidOperationException(
                "There is already an open claim on this policy. " +
                "Please wait for it to be resolved before raising another.");

        // Validate Nominee Details matching exactly what was registered
        var isNomineeValid = policy.Nominees != null && policy.Nominees.Any(n => 
            n.FullName.Equals(dto.NomineeName, StringComparison.OrdinalIgnoreCase) &&
            n.Relationship.Equals(dto.NomineeRelationship, StringComparison.OrdinalIgnoreCase));

        if (!isNomineeValid)
        {
            throw new InvalidOperationException("The nominee details provided do not match the registered nominee information for this policy.");
        }

        // Auto-assign a claims officer
        var officerId = await _officerAssignment.AssignOfficerAsync();
        var claimNumber = await _claimRepo.GenerateClaimNumberAsync();

        var claim = new Claim
        {
            PolicyId = policy.Id,
            CustomerId = policy.CustomerId,
            ClaimsOfficerId = officerId,
            ClaimNumber = claimNumber,
            Status = ClaimStatus.Submitted,
            ClaimReason = $"Date of Death: {dto.DateOfDeath:yyyy-MM-dd}. Cause: {dto.CauseOfDeath}",
            ClaimAmount = policy.SumAssured, // Full sum assured
            // Bank details provided by claimant for settlement transfer
            BankAccountName = dto.BankAccountName,
            BankAccountNumber = dto.BankAccountNumber,
            BankIfscCode = dto.BankIfscCode,
            SubmittedAt = DateTime.UtcNow,
            AssignedAt = DateTime.UtcNow
        };

        await _claimRepo.CreateAsync(claim);

        await _notificationService.CreateNotificationAsync(policy.CustomerId, $"A claim request '{claimNumber}' has been submitted for this policy.");
        if (officerId > 0)
        {
            await _notificationService.CreateNotificationAsync(officerId, $"A new claim '{claimNumber}' has been assigned to you for review.");
        }

        // Handle mandatory death certificate upload
        if (dto.DeathCertificate != null)
        {
            await UploadClaimDocumentAsync(claim.Id, dto.DeathCertificate);
        }

        var created = await _claimRepo.GetByIdWithDetailsAsync(claim.Id);
        return MapToDto(created!);
    }

    // ── Customer: Upload Claim Document ───────────────────────────────────

    public async Task<ClaimResponseDto> UploadClaimDocumentAsync(
        int claimId, ClaimDocumentDto dto)
    {
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        if (claim.Status == ClaimStatus.Settled ||
            claim.Status == ClaimStatus.Rejected)
            throw new InvalidOperationException(
                "Cannot upload documents for a closed claim.");

        var document = new ClaimDocument
        {
            ClaimId = claimId,
            DocumentType = dto.DocumentType,
            FileName = dto.FileName,
            FilePath = "",
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(dto.FileBase64))
        {
            var base64Data = dto.FileBase64.Split(',').Last();
            var fileBytes = Convert.FromBase64String(base64Data);

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "claims", claimId.ToString());
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, dto.FileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);
            document.FilePath = $"/uploads/claims/{claimId}/{dto.FileName}";
        }

        await _claimRepo.AddDocumentAsync(document);

        var updated = await _claimRepo.GetByIdWithDetailsAsync(claimId);
        return MapToDto(updated!);
    }

    // ── Customer: View My Claims ───────────────────────────────────────────

    public async Task<List<ClaimResponseDto>> GetMyClaimsAsync(int customerId)
    {
        var claims = await _claimRepo.GetByCustomerIdAsync(customerId);
        return claims.Select(MapToDto).ToList();
    }

    // ── Claims Officer: View Assigned Claims ──────────────────────────────

    public async Task<List<ClaimResponseDto>> GetOfficerClaimsAsync(int officerId)
    {
        var claims = await _claimRepo.GetByOfficerIdAsync(officerId);
        return claims.Select(MapToDto).ToList();
    }

    // ── Claims Officer: Start Review ──────────────────────────────────────

    public async Task<ClaimResponseDto> StartReviewAsync(int claimId, int officerId)
    {
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        EnsureOfficerOwnsClaim(claim, officerId);

        if (claim.Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                "Only Submitted claims can be moved to Under Review.");

        claim.Status = ClaimStatus.UnderReview;
        claim.ReviewStartedAt = DateTime.UtcNow;

        await _claimRepo.UpdateAsync(claim);
        return MapToDto(claim);
    }

    // ── Claims Officer: Make Decision ─────────────────────────────────────

    public async Task<ClaimResponseDto> MakeDecisionAsync(
        int claimId, int officerId, ClaimDecisionDto dto)
    {
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        EnsureOfficerOwnsClaim(claim, officerId);

        if (claim.Status != ClaimStatus.UnderReview)
            throw new InvalidOperationException(
                "Decision can only be made on claims that are Under Review.");

        // Verify policy is still valid before approving
        var policy = await _policyRepo.GetByIdAsync(claim.PolicyId)!;

        if (dto.IsApproved)
        {
            // Verify payment history — cannot settle if policy is lapsed
            if (policy!.Status == PolicyStatus.Lapsed ||
                policy.Status == PolicyStatus.Suspended)
                throw new InvalidOperationException(
                    "Cannot approve claim — policy is Suspended or Lapsed.");

            if (dto.SettledAmount == null)
                throw new InvalidOperationException(
                    "Settled amount is required when approving a claim.");

            if (dto.SettledAmount > claim.ClaimAmount)
                throw new InvalidOperationException(
                    "Settled amount cannot exceed the claimed amount.");

            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAt = DateTime.UtcNow;
            claim.OfficerRemarks = dto.OfficerRemarks;
            claim.SettledAmount = dto.SettledAmount;

            await _claimRepo.UpdateAsync(claim);

            // Immediately settle after approval — generate a bank transfer reference
            claim.Status = ClaimStatus.Settled;
            claim.SettledAt = DateTime.UtcNow;
            claim.TransferReference = $"TXN-{claim.ClaimNumber}-{DateTime.UtcNow:yyyyMMddHHmm}";
            await _claimRepo.UpdateAsync(claim);

            await _notificationService.CreateNotificationAsync(claim.CustomerId, $"Your claim '{claim.ClaimNumber}' has been approved and settled for ₹{claim.SettledAmount:N0}. Transfer Ref: {claim.TransferReference}");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new InvalidOperationException(
                    "Rejection reason is required when rejecting a claim.");

            claim.Status = ClaimStatus.Rejected;
            claim.RejectedAt = DateTime.UtcNow;
            claim.RejectionReason = dto.RejectionReason;
            claim.OfficerRemarks = dto.OfficerRemarks;

            await _claimRepo.UpdateAsync(claim);

            await _notificationService.CreateNotificationAsync(claim.CustomerId, $"Your claim '{claim.ClaimNumber}' has been rejected. Reason: {claim.RejectionReason}");
        }

        return MapToDto(claim);
    }

    // ── Shared: Get Single Claim (role-aware) ─────────────────────────────

    public async Task<ClaimResponseDto> GetClaimDetailsAsync(
        int claimId, int requestingUserId, string role)
    {
        var claim = await _claimRepo.GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        var hasAccess = role switch
        {
            "Admin" => true,
            "Customer" => claim.CustomerId == requestingUserId,
            "ClaimsOfficer" => claim.ClaimsOfficerId == requestingUserId,
            _ => false
        };

        if (!hasAccess)
            throw new UnauthorizedAccessException(
                "You do not have access to this claim.");

        return MapToDto(claim);
    }

    // ── Admin: All Claims ─────────────────────────────────────────────────

    public async Task<List<ClaimResponseDto>> GetAllClaimsAsync(
        ClaimStatus? status = null)
    {
        var claims = await _claimRepo.GetAllAsync(status);
        return claims.Select(MapToDto).ToList();
    }

    // ── Private Helpers ───────────────────────────────────────────────────

    private static void EnsureOfficerOwnsClaim(Claim claim, int officerId)
    {
        if (claim.ClaimsOfficerId != officerId)
            throw new UnauthorizedAccessException(
                "You are not assigned to this claim.");
    }

    private static ClaimResponseDto MapToDto(Claim c)
    {
        var policy = c.Policy;
        var invoices = policy?.Invoices?.ToList() ?? new List<Invoice>();
        var paidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid);
        var overdueInvoices = invoices.Count(i =>
            i.Status == InvoiceStatus.Overdue ||
            i.Status == InvoiceStatus.Grace);
        var totalPremiumPaid = invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Sum(i => i.AmountDue);

        return new ClaimResponseDto
        {
            ClaimId = c.Id,
            ClaimNumber = c.ClaimNumber,
            Status = c.Status.ToString(),
            PolicyId = c.PolicyId,
            PolicyNumber = policy?.PolicyNumber ?? string.Empty,
            PlanName = policy?.InsurancePlan?.PlanName ?? string.Empty,
            SumAssured = policy?.SumAssured ?? 0,
            TenureYears = policy?.TenureYears ?? 0,
            RiskCategory = policy?.RiskCategory,
            PremiumAmount = policy?.PremiumAmount,
            PolicyActiveFrom = policy?.ActiveFrom,
            PolicyActiveTo = policy?.ActiveTo,
            CustomerId = c.CustomerId,
            CustomerName = c.Customer?.FullName ?? string.Empty,
            CustomerEmail = c.Customer?.Email ?? string.Empty,
            ClaimsOfficerId = c.ClaimsOfficerId,
            ClaimsOfficerName = c.ClaimsOfficer?.FullName,
            ClaimReason = c.ClaimReason,
            ClaimAmount = c.ClaimAmount,
            SettledAmount = c.SettledAmount,
            OfficerRemarks = c.OfficerRemarks,
            RejectionReason = c.RejectionReason,
            BankAccountName = c.BankAccountName,
            BankAccountNumber = c.BankAccountNumber,
            BankIfscCode = c.BankIfscCode,
            TransferReference = c.TransferReference,
            Nominees = policy?.Nominees?.Select(n => new ClaimNomineeDto
            {
                FullName = n.FullName,
                Relationship = n.Relationship,
                Age = n.Age,
                ContactNumber = n.ContactNumber,
                AllocationPercentage = n.AllocationPercentage
            }).ToList() ?? new List<ClaimNomineeDto>(),
            TotalInvoices = invoices.Count,
            PaidInvoices = paidInvoices,
            OverdueInvoices = overdueInvoices,
            TotalPremiumPaid = totalPremiumPaid,
            Documents = c.ClaimDocuments.Select(d => new ClaimDocumentResponseDto
            {
                DocumentId = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FilePath = d.FilePath,
                Status = d.Status.ToString(),
                UploadedAt = d.UploadedAt
            }).ToList(),
            SubmittedAt = c.SubmittedAt,
            AssignedAt = c.AssignedAt,
            ReviewStartedAt = c.ReviewStartedAt,
            ApprovedAt = c.ApprovedAt,
            SettledAt = c.SettledAt,
            RejectedAt = c.RejectedAt
        };
    }
}