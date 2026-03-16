using Application.DTOs.Claim;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AutomatedMaturityService : IAutomatedMaturityService
{
    private readonly IPolicyRepository _policyRepo;
    private readonly IClaimRepository _claimRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IPremiumCalculationService _premiumCalc;
    private readonly IEmailService _emailService;

    private readonly ILogger<AutomatedMaturityService> _logger;

    public AutomatedMaturityService(
        IPolicyRepository policyRepo,
        IClaimRepository claimRepo,
        IUserRepository userRepo,
        INotificationService notificationService,
        IPremiumCalculationService premiumCalc,
        IEmailService emailService,
        ILogger<AutomatedMaturityService> logger)

    {
        _policyRepo = policyRepo;
        _claimRepo = claimRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _premiumCalc = premiumCalc;
        _logger = logger;
        _emailService = emailService;
    }


    public async Task<int> ProcessMaturitiesAsync()
    {
        _logger.LogInformation("Starting Automated Maturity Processing Job.");

        // Find all active Endowment policies that have reached their ActiveTo date (Maturity Date)
        // We only fetch Active policies to ensure we don't pay out suspended, lapsed, or cancelled policies.
        var policies = await _policyRepo.GetAllAsync(PolicyStatus.Active);
        
        var maturedPolicies = policies
            .Where(p => p.InsurancePlan?.PlanType == PlanType.Endowment && 
                        p.ActiveTo.HasValue && 
                        p.ActiveTo.Value.Date <= DateTime.UtcNow.Date)
            .ToList();

        if (!maturedPolicies.Any())
        {
            _logger.LogInformation("No matured policies found today.");
            return 0;
        }

        int processedCount = 0;

        foreach (var policy in maturedPolicies)
        {
            try
            {
                // Check if a maturity claim was already raised manually or by previous job run
                var existingClaims = await _claimRepo.GetByCustomerIdAsync(policy.CustomerId);
                if (existingClaims.Any(c => c.PolicyId == policy.Id && c.Type == ClaimType.Maturity))
                {
                    continue; // Skip, already processed
                }

                _logger.LogInformation($"Processing matured policy: {policy.PolicyNumber} for Customer ID: {policy.CustomerId}");

                var customer = await _userRepo.GetByIdAsync(policy.CustomerId);
                if (customer == null) continue;

                // Calculate the exact payout incorporating the accrued bonus
                var calcResult = _premiumCalc.Calculate(policy.InsurancePlan!, policy.SumAssured, policy.TenureYears, policy.RiskCategory ?? "Standard");
                var maturityBenefit = calcResult.MaturityBenefit > 0 ? calcResult.MaturityBenefit : policy.SumAssured;

                // Create the Automated Maturity Claim
                var claimNumber = await _claimRepo.GenerateClaimNumberAsync();
                
                var claim = new Claim
                {
                    PolicyId = policy.Id,
                    CustomerId = policy.CustomerId,
                    ClaimsOfficerId = null, // Automated!
                    ClaimNumber = claimNumber,
                    Type = ClaimType.Maturity,
                    ClaimReason = "Automated Policy Maturity Payout",
                    ClaimAmount = maturityBenefit,
                    
                    // Directly approve and settle
                    Status = ClaimStatus.Settled,
                    SettledAmount = maturityBenefit,
                    OfficerRemarks = "System Auto-Settlement on Policy Maturity Date",
                    
                    // Destination bank details sourced from Customer Profile (fallback to blank if missing)
                    BankAccountName = customer.BankAccountName ?? customer.FullName,
                    BankAccountNumber = customer.BankAccountNumber ?? "PENDING-UPDATE",
                    BankIfscCode = customer.BankIfscCode ?? "PENDING",
                    TransferReference = $"AUTOTXN-{claimNumber}-{DateTime.UtcNow:yyyyMMddHHmm}",
                    
                    SubmittedAt = DateTime.UtcNow,
                    ApprovedAt = DateTime.UtcNow,
                    SettledAt = DateTime.UtcNow
                };

                await _claimRepo.CreateAsync(claim);

                // Retire the policy as Settled
                policy.Status = PolicyStatus.Settled;
                await _policyRepo.UpdateAsync(policy);

                // Notify Customer
                var message = $"Congratulations! Your Endowment Policy {policy.PolicyNumber} has matured. " +
                              $"A payout of ₹{maturityBenefit:N0} has been automatically processed and transferred to your account (Ref: {claim.TransferReference}).";
                await _notificationService.CreateNotificationAsync(customer.Id, message);

                // Send Maturity Email
                await _emailService.SendCustomerMaturityEmail(
                    customer.Email,
                    customer.FullName,
                    policy.PolicyNumber,
                    maturityBenefit,
                    claim.TransferReference,
                    claim.SettledAt ?? DateTime.UtcNow,
                    claim.BankAccountName ?? "Registered Bank",
                    customer.FullName,
                    claim.BankAccountNumber ?? "XXXX",
                    claim.BankIfscCode ?? ""
                );


                // Special Feature: After settlements, if ALL policies for this customer are now Settled, deactivate the account.
                var allCustomerPolicies = await _policyRepo.GetByCustomerIdAsync(customer.Id);
                if (allCustomerPolicies.All(p => p.Status == PolicyStatus.Settled))
                {
                    customer.IsActive = false;
                    await _userRepo.UpdateAsync(customer);
                    await _notificationService.CreateNotificationAsync(customer.Id, "All your policies have been fully settled via automated maturity payout. Your account has been closed as per policy guidelines.");
                    _logger.LogInformation($"Customer {customer.Id} deactivated as all policies are now settled.");
                }

                processedCount++;
                _logger.LogInformation($"Successfully settled maturity for policy {policy.PolicyNumber}. Payout: {maturityBenefit}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process maturity for policy {policy.PolicyNumber}.");
            }
        }

        _logger.LogInformation($"Automated Maturity Processing Job completed. Processed {processedCount} policies.");
        return processedCount;
    }
}
