using System.Security.Claims;
using Application.DTOs.Endorsement;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EndorsementController : ControllerBase
{
    private readonly IEndorsementService _endorsementService;

    public EndorsementController(IEndorsementService endorsementService)
    {
        _endorsementService = endorsementService;
    }

    // ── Customer ──────────────────────────────────────────────────────────

    // POST api/endorsement/request/{type}
    [HttpPost("request/{type}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RequestEndorsement(string type, [FromBody] System.Text.Json.JsonElement payload)
    {
        var customerId = GetCurrentUserId();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = payload.GetRawText();
        
        switch (type.ToLower())
        {
            case "address":
                var addressDto = System.Text.Json.JsonSerializer.Deserialize<RequestAddressChangeDto>(json, options);
                return Ok(await _endorsementService.RequestAddressChangeAsync(customerId, addressDto!));
            
            case "nominee":
                var nomineeDto = System.Text.Json.JsonSerializer.Deserialize<RequestNomineeChangeDto>(json, options);
                return Ok(await _endorsementService.RequestNomineeChangeAsync(customerId, nomineeDto!));
                
            case "sumassured":
                var sumDto = System.Text.Json.JsonSerializer.Deserialize<RequestSumAssuredIncreaseDto>(json, options);
                return Ok(await _endorsementService.RequestSumAssuredIncreaseAsync(customerId, sumDto!));
                
            default:
                return BadRequest("Invalid endorsement type.");
        }
    }

    // GET api/endorsement
    // GET api/endorsement?status=Requested
    [HttpGet]
    [Authorize(Roles = "Admin,Customer,Agent")]
    public async Task<IActionResult> GetEndorsements([FromQuery] EndorsementStatus? status = null)
    {
        var role = GetCurrentUserRole();
        
        if (role == "Customer")
        {
            var customerId = GetCurrentUserId();
            var result = await _endorsementService.GetMyEndorsementsAsync(customerId);
            return Ok(result);
        }
        else if (role == "Agent")
        {
            var agentId = GetCurrentUserId();
            var result = await _endorsementService.GetPendingEndorsementsAsync(agentId);
            return Ok(result);
        }
        else if (role == "Admin")
        {
            var result = await _endorsementService.GetAllAsync(status);
            return Ok(result);
        }
        
        return Forbid();
    }

    // POST api/endorsement/{endorsementId}/decision
    [HttpPost("{endorsementId:int}/decision")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> MakeDecision(
        int endorsementId, [FromBody] EndorsementDecisionDto dto)
    {
        var agentId = GetCurrentUserId();
        var result = await _endorsementService
            .MakeDecisionAsync(endorsementId, agentId, dto);
        return Ok(result);
    }

    // ── Shared ────────────────────────────────────────────────────────────

    // GET api/endorsement/{endorsementId}
    [HttpGet("{endorsementId:int}")]
    [Authorize(Roles = "Admin,Customer,Agent")]
    public async Task<IActionResult> GetById(int endorsementId)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var result = await _endorsementService
            .GetByIdAsync(endorsementId, userId, role);
        return Ok(result);
    }

    // GET api/endorsement/policy/{policyId}
    [HttpGet("policy/{policyId:int}")]
    [Authorize(Roles = "Admin,Customer,Agent")]
    public async Task<IActionResult> GetByPolicy(int policyId)
    {
        var result = await _endorsementService.GetByPolicyIdAsync(policyId);
        return Ok(result);
    }



    // ── Helpers ───────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(
            System.Security.Claims.ClaimTypes.NameIdentifier)!);

    private string GetCurrentUserRole()
        => User.FindFirstValue(System.Security.Claims.ClaimTypes.Role)!;
}