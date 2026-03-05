using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<AgentDashboardDto> GetAgentDashboardAsync(int agentId);
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(int customerId);
    Task<ClaimsOfficerDashboardDto> GetClaimsOfficerDashboardAsync(int officerId);
}