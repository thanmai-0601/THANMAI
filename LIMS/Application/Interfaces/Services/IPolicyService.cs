using Application.DTOs.Policy;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IPolicyService
{
    // Customer
    Task<PolicyResponseDto> RequestPolicyAsync(int customerId, RequestPolicyDto dto);
    Task<List<PolicyResponseDto>> GetMyPoliciesAsync(int customerId);

    // Agent
    Task<List<PolicyResponseDto>> GetAgentPoliciesAsync(int agentId);
    Task<PolicyResponseDto> GetPolicyDetailsAsync(int policyId, int requestingUserId, string role);

    // Admin
    Task<List<PolicyResponseDto>> GetAllPoliciesAsync(PolicyStatus? status = null);

    Task<object> UploadDocumentAsync(
    int policyId,
    int customerId,
    string documentType,
    string fileName,
    string fileBase64);

    Task<object> SubmitNomineesAsync(
    int policyId,
    int customerId,
    SubmitNomineesDto dto);
}