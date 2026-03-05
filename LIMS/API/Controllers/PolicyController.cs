using Application.DTOs.Policy;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PolicyController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly IPolicyService _policyService;
    private readonly IAgentPolicyService 
        _agentPolicyService;

    public PolicyController(IPlanService planService, IPolicyService policyService,
        IAgentPolicyService agentPolicyService)
    {
        _planService = planService;
        _policyService = policyService;
        _agentPolicyService = agentPolicyService;
    }

    // ── Plans (browsing) ────────────────────────────────────────────────────

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans()
    {
        var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        var plans = await _planService.GetAllPlansAsync(includeInactive: isAdmin);
        return Ok(plans);
    }

    [HttpGet("plans/{id:int}")]
    public async Task<IActionResult> GetPlanDetails(int id)
    {
        var plan = await _planService.GetPlanByIdAsync(id);
        return Ok(plan);
    }

    // ── Customer: Request to Buy ─────────────────────────────────────────────

    [HttpPost("request")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RequestPolicy([FromBody] RequestPolicyDto dto)
    {
        var customerId = GetCurrentUserId();
        var result = await _policyService.RequestPolicyAsync(customerId, dto);
        return Ok(result);
    }

    // GET api/policy
    // GET api/policy?status=Pending
    [HttpGet]
    [Authorize(Roles = "Admin,Customer,Agent")]
    public async Task<IActionResult> GetPolicies([FromQuery] PolicyStatus? status = null)
    {
        var role = GetCurrentUserRole();
        
        if (role == "Customer")
        {
            var customerId = GetCurrentUserId();
            var result = await _policyService.GetMyPoliciesAsync(customerId);
            return Ok(result);
        }
        else if (role == "Agent")
        {
            var agentId = GetCurrentUserId();
            var result = await _policyService.GetAgentPoliciesAsync(agentId);
            return Ok(result);
        }
        else if (role == "Admin")
        {
            var result = await _policyService.GetAllPoliciesAsync(status);
            return Ok(result);
        }
        
        return Forbid();
    }

    // GET api/policy/{policyId}/premium-preview
    [HttpGet("{policyId:int}/premium-preview")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetPremiumPreview(int policyId)
    {
        var customerId = GetCurrentUserId();
        var policy = await _policyService.GetPolicyDetailsAsync(
            policyId, customerId, "Customer");

        if (policy.PremiumAmount == null)
            return BadRequest(new { message = "Premium has not been calculated yet. Please wait for agent review." });

        // Build preview from existing policy data
        var preview = new PremiumPreviewDto
        {
            PolicyNumber = policy.PolicyNumber,
            PlanName = policy.PlanName,
            SumAssured = policy.SumAssured,
            TenureYears = policy.TenureYears,
            RiskCategory = policy.RiskCategory ?? string.Empty,
            AnnualPremium = policy.PremiumAmount ?? 0,
            MonthlyPremium = Math.Round((policy.PremiumAmount ?? 0) / 12, 2),
            QuarterlyPremium = Math.Round((policy.PremiumAmount ?? 0) / 4, 2),
            TotalPayableOverTenure = Math.Round(
                (policy.PremiumAmount ?? 0) * policy.TenureYears, 2),
            SumAssuredOnDeath = policy.SumAssured,
            SumAssuredOnMaturity = policy.SumAssured,
            AgentRemarks = policy.AgentRemarks ?? string.Empty,
            Status = policy.Status,
            Benefits = new List<string>
        {
            "Death Benefit: Full sum assured paid to nominees",
            "Maturity Benefit: Sum assured returned on completion",
            "Tax Benefit: Premium eligible under Section 80C",
            "Grace Period: 30 days after due date"
        }
        };

        return Ok(preview);
    }

    // ── Customer: Nominee Submission ───────────────────────────────────────

    [HttpPost("{policyId:int}/nominees")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SubmitNominees(
        int policyId,
        [FromBody] SubmitNomineesDto dto)
    {
        var customerId = GetCurrentUserId();
        var result = await _policyService.SubmitNomineesAsync(
            policyId, customerId, dto);

        return Ok(result);
    }

    // ── Customer: Upload Documents ─────────────────────────────────────────

    [HttpPost("{policyId:int}/documents")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UploadDocument(
        int policyId,
        [FromBody] UploadDocumentDto dto)
    {
        var customerId = GetCurrentUserId();
        var result = await _policyService.UploadDocumentAsync(
            policyId,
            customerId,
            dto.DocumentType,
            dto.FileName,
            dto.FileBase64);

        return Ok(result);
    }

    // ── Agent: Actions ──────────────────────────────────────────────────────

    [HttpPost("{policyId:int}/calculate-premium")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> CalculatePremium(
        int policyId, [FromBody] AgentPremiumCalculationDto dto)
    {
        var agentId = GetCurrentUserId();
        var result = await _agentPolicyService.CalculatePremiumAsync(
            policyId, agentId, dto);
        return Ok(result);
    }

    [HttpPost("{policyId:int}/decision")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> MakeDecision(
        int policyId, [FromBody] PolicyDecisionDto dto)
    {
        var agentId = GetCurrentUserId();
        var result = await _agentPolicyService.MakeDecisionAsync(
            policyId, agentId, dto);
        return Ok(result);
    }

    [HttpGet("{policyId:int}/nominees-agent")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> GetNominees(int policyId)
    {
        var result = await _agentPolicyService.GetNomineesAsync(policyId);
        return Ok(result);
    }

    // ── Admin: Plan Management ──────────────────────────────────────────────

    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto)
    {
        var result = await _planService.CreatePlanAsync(dto);
        return CreatedAtAction(nameof(GetPlanDetails), new { id = result.PlanId }, result);
    }

    [HttpPut("plans/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanDto dto)
    {
        var result = await _planService.UpdatePlanAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("plans/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        await _planService.DeletePlanAsync(id);
        return Ok(new { Message = "Plan deleted successfully." });
    }

    // ── Shared: Single Policy View ─────────────────────────────────────────

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Agent,Customer")]
    public async Task<IActionResult> GetPolicy(int id)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var result = await _policyService.GetPolicyDetailsAsync(id, userId, role);
        return Ok(result);
    }



    // ── Helpers ─────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetCurrentUserRole()
        => User.FindFirstValue(ClaimTypes.Role)!;
}