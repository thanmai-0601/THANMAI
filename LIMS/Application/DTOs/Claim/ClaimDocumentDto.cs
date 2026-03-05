using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Claim;

public class ClaimDocumentDto
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileBase64 { get; set; } = string.Empty;
}