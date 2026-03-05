using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services;

public class AgentAssignmentService : IAgentAssignmentService
{
    private readonly IUserRepository _userRepo;
    private readonly IPolicyRepository _policyRepo;

    public AgentAssignmentService(
        IUserRepository userRepo,
        IPolicyRepository policyRepo)
    {
        _userRepo = userRepo;
        _policyRepo = policyRepo;
    }

    public async Task<int> AssignAgentAsync()
    {
        // Step 1 — Get all active agents
        var agents = await _userRepo.GetActiveAgentsAsync();

        if (!agents.Any())
            throw new InvalidOperationException(
                "No active agents available. Please contact admin.");

        var agentWorkloads = new List<(int AgentId, int ActiveCount, DateTime? LastAssigned)>();

        foreach (var agent in agents)
        {
            var activeCount = await _policyRepo.GetActiveCountByAgentAsync(agent.Id);

            var lastAssigned = await _policyRepo.GetLastAssignmentDateAsync(agent.Id);


            agentWorkloads.Add((agent.Id, activeCount, lastAssigned));
        }

        var selected = agentWorkloads
            .OrderBy(a => a.ActiveCount)
            .ThenBy(a => a.LastAssigned ?? DateTime.MinValue)
            .First();

        return selected.AgentId;
    }
}