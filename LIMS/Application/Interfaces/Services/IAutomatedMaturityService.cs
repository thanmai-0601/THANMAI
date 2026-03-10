namespace Application.Interfaces.Services;

public interface IAutomatedMaturityService
{
    Task<int> ProcessMaturitiesAsync();
}
