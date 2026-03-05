using Application.DTOs.Policy;

namespace Application.Interfaces.Services;

public interface IAgentPolicyService
{
    // Agent fills eligibility and calculates premium
    Task<PremiumCalculationResultDto> CalculatePremiumAsync(
        int policyId,
        int agentId,
        AgentPremiumCalculationDto dto);

    // Customer submits nominees
    Task<List<NomineeResponseDto>> SubmitNomineesAsync(
        int policyId, int customerId, SubmitNomineesDto dto);

    // Customer uploads document (returns updated doc info)
    Task<DocumentResponseDto> UploadDocumentAsync(
        int policyId, int customerId, string documentType,
        string fileName, string filePath);

    // Agent approves or rejects
    Task<PolicyResponseDto> MakeDecisionAsync(
        int policyId, int agentId, PolicyDecisionDto dto);

    // Get nominees for a policy
    Task<List<NomineeResponseDto>> GetNomineesAsync(int policyId);
}