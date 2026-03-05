using Application.DTOs.Claim;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IClaimService
{
    // Customer
    Task<ClaimResponseDto> RaiseClaimAsync(RaiseClaimDto dto);
    Task<ClaimResponseDto> UploadClaimDocumentAsync(
        int claimId, ClaimDocumentDto dto);
    Task<List<ClaimResponseDto>> GetMyClaimsAsync(int customerId);

    // Claims Officer
    Task<List<ClaimResponseDto>> GetOfficerClaimsAsync(int officerId);
    Task<ClaimResponseDto> StartReviewAsync(int claimId, int officerId);
    Task<ClaimResponseDto> MakeDecisionAsync(
        int claimId, int officerId, ClaimDecisionDto dto);

    // Shared
    Task<ClaimResponseDto> GetClaimDetailsAsync(
        int claimId, int requestingUserId, string role);

    // Admin
    Task<List<ClaimResponseDto>> GetAllClaimsAsync(ClaimStatus? status = null);
}