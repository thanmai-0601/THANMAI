using Domain.Common;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Entities;

public class Document : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty; // "IdentityProof", "AddressProof", "IncomeProof"
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}