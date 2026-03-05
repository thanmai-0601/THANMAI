namespace Application.Interfaces.Services
{
    public interface IAgentAssignmentService
    {
        Task<int> AssignAgentAsync();  // returns the assigned AgentId
    }
}
