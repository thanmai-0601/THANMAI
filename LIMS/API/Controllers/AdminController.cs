using Application.DTOs.Auth;
using Application.DTOs.Policy;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPlanService _planService;

    public AdminController(IAuthService authService,IPlanService planService)
    {
        _authService = authService;
        _planService = planService;
    }

    // ── Staff Creation ──────────────────────────────────────────────────────

    // POST api/admin/create-staff/{role}
    [HttpPost("create-staff/{role}")]
    public async Task<IActionResult> CreateStaff(UserRole role, [FromBody] CreateStaffDto dto)
    {
        var result = await _authService.CreateStaffAsync(dto, role);
        return Ok(result);
    }

    // ── User Management ─────────────────────────────────────────────────────

    // GET api/admin/users
    // GET api/admin/users?role=Agent
    // GET api/admin/users?role=ClaimsOfficer
    // GET api/admin/users?role=Customer
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserRole? role = null)
    {
        var users = await _authService.GetAllUsersAsync(role);
        return Ok(users);
    }

    // GET api/admin/users/5
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        return Ok(user);
    }

    // PUT api/admin/users/5
    // Edit Agent or ClaimsOfficer details (Name, Email, Phone, Role)
    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        var result = await _authService.UpdateStaffAsync(id, dto);
        return Ok(result);
    }

    // DELETE api/admin/users/5
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _authService.DeleteUserAsync(id);
        return Ok(new { Message = "User deleted successfully." });
    }

    // PUT api/admin/users/5/toggle-status
    [HttpPut("users/{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        await _authService.ToggleUserStatusAsync(id);
        return Ok(new { Message = "User status toggled successfully." });
    }

}