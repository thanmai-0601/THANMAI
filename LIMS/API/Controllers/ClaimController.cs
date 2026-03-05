using System.Security.Claims;
using Application.DTOs.Claim;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    // ── Customer endpoints ────────────────────────────────────────────────

    // POST api/claim/raise
    [HttpPost("raise")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RaiseClaim([FromBody] RaiseClaimDto dto)
    {
        var result = await _claimService.RaiseClaimAsync(dto);
        return Ok(result);
    }

    // POST api/claim/{claimId}/documents
    [HttpPost("{claimId:int}/documents")]
    [Authorize]
    public async Task<IActionResult> UploadDocument(
        int claimId, [FromBody] ClaimDocumentDto dto)
    {
        var result = await _claimService
            .UploadClaimDocumentAsync(claimId, dto);
        return Ok(result);
    }

    // GET api/claim
    // GET api/claim?status=Submitted
    [HttpGet]
    [Authorize(Roles = "Admin,Customer,ClaimsOfficer")]
    public async Task<IActionResult> GetClaims([FromQuery] ClaimStatus? status = null)
    {
        var role = GetCurrentUserRole();
        
        if (role == "Customer")
        {
            var customerId = GetCurrentUserId();
            var result = await _claimService.GetMyClaimsAsync(customerId);
            return Ok(result);
        }
        else if (role == "ClaimsOfficer")
        {
            var officerId = GetCurrentUserId();
            var result = await _claimService.GetOfficerClaimsAsync(officerId);
            return Ok(result);
        }
        else if (role == "Admin")
        {
            var result = await _claimService.GetAllClaimsAsync(status);
            return Ok(result);
        }
        
        return Forbid();
    }

    // PATCH api/claim/{claimId}/start-review
    [HttpPatch("{claimId:int}/start-review")]
    [Authorize(Roles = "ClaimsOfficer")]
    public async Task<IActionResult> StartReview(int claimId)
    {
        var officerId = GetCurrentUserId();
        var result = await _claimService.StartReviewAsync(claimId, officerId);
        return Ok(result);
    }

    // POST api/claim/{claimId}/decision
    [HttpPost("{claimId:int}/decision")]
    [Authorize(Roles = "ClaimsOfficer")]
    public async Task<IActionResult> MakeDecision(
        int claimId, [FromBody] ClaimDecisionDto dto)
    {
        var officerId = GetCurrentUserId();
        var result = await _claimService.MakeDecisionAsync(claimId, officerId, dto);
        return Ok(result);
    }

    // ── Shared ────────────────────────────────────────────────────────────

    // GET api/claim/{claimId}
    [HttpGet("{claimId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetClaimDetails(int claimId)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : 0;
        var role = User.Identity?.IsAuthenticated == true ? GetCurrentUserRole() : "Anonymous";
        var result = await _claimService
            .GetClaimDetailsAsync(claimId, userId, role);
        return Ok(result);
    }

    // ── Admin ─────────────────────────────────────────────────────────────



    // ── Helpers ───────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(
            System.Security.Claims.ClaimTypes.NameIdentifier)!);

    private string GetCurrentUserRole()
        => User.FindFirstValue(System.Security.Claims.ClaimTypes.Role)!;
}