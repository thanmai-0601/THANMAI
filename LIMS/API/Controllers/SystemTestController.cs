using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemTestController : ControllerBase
{
    private readonly IAutomatedMaturityService _maturityService;
    private readonly IPolicyRepository _policyRepo;

    public SystemTestController(
        IAutomatedMaturityService maturityService,
        IPolicyRepository policyRepo)
    {
        _maturityService = maturityService;
        _policyRepo = policyRepo;
    }

    /// <summary>
    /// Forces a policy to instantly mature by setting ActiveTo to yesterday.
    /// Good for testing Endowment payouts without waiting 10 years!
    /// </summary>
    [HttpPost("fast-forward-policy/{policyId}")]
    public async Task<IActionResult> FastForwardPolicy(int policyId)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId);
        if (policy == null) return NotFound("Policy not found.");

        policy.ActiveTo = DateTime.UtcNow.AddDays(-1);
        await _policyRepo.UpdateAsync(policy);

        return Ok(new { Message = $"Policy {policy.PolicyNumber} has been artificially aged and is now Matured.", policy.ActiveTo });
    }

    /// <summary>
    /// Instantly triggers the background maturity job instead of waiting for 24 hours.
    /// </summary>
    [HttpPost("trigger-maturity-job")]
    public async Task<IActionResult> TriggerMaturityJob()
    {
        int processedCount = await _maturityService.ProcessMaturitiesAsync();
        return Ok(new { Message = $"Maturity job triggered successfully. Processed {processedCount} policies." });
    }
}
