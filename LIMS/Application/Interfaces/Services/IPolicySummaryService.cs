using System.Threading.Tasks;

namespace Application.Interfaces.Services;

public interface IPolicySummaryService
{
    Task<string> GeneratePolicySummaryAsync(int policyId);
}
