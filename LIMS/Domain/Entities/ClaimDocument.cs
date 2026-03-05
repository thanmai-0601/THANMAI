using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class ClaimDocument : BaseEntity
{
    public int ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}