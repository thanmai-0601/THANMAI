using System.Threading.Tasks;

namespace Application.Interfaces.Services;

public interface IClaimSummaryService
{
    Task<string> GenerateClaimSummaryAsync(int claimId);
}
