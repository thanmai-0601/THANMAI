using Application.DTOs.Endorsement;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IEndorsementService
{
    // Customer requests
    Task<EndorsementResponseDto> RequestAddressChangeAsync(
        int customerId, RequestAddressChangeDto dto);
    Task<EndorsementResponseDto> RequestNomineeChangeAsync(
        int customerId, RequestNomineeChangeDto dto);
    Task<EndorsementResponseDto> RequestSumAssuredIncreaseAsync(
        int customerId, RequestSumAssuredIncreaseDto dto);

    Task<List<EndorsementResponseDto>> GetMyEndorsementsAsync(int customerId);

    // Agent review
    Task<List<EndorsementResponseDto>> GetPendingEndorsementsAsync(int agentId);
    Task<EndorsementResponseDto> MakeDecisionAsync(
        int endorsementId, int agentId, EndorsementDecisionDto dto);

    // Shared
    Task<EndorsementResponseDto> GetByIdAsync(
        int endorsementId, int requestingUserId, string role);
    Task<List<EndorsementResponseDto>> GetByPolicyIdAsync(int policyId);

    // Admin
    Task<List<EndorsementResponseDto>> GetAllAsync(EndorsementStatus? status = null);
}