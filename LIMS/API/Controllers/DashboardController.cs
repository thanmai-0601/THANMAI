using System.Security.Claims;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET api/dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        
        switch (role)
        {
            case "Admin":
                var adminResult = await _dashboardService.GetAdminDashboardAsync();
                return Ok(adminResult);
            
            case "Agent":
                var agentId = GetCurrentUserId();
                var agentResult = await _dashboardService.GetAgentDashboardAsync(agentId);
                return Ok(agentResult);
                
            case "Customer":
                var customerId = GetCurrentUserId();
                var customerResult = await _dashboardService.GetCustomerDashboardAsync(customerId);
                return Ok(customerResult);
                
            case "ClaimsOfficer":
                var officerId = GetCurrentUserId();
                var officerResult = await _dashboardService.GetClaimsOfficerDashboardAsync(officerId);
                return Ok(officerResult);
                
            default:
                return Forbid();
        }
    }

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}