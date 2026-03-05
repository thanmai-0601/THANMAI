
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

public class UploadDocumentDto
{
    [Required]
    [RegularExpression("^(Address Proof|Income Proof|Nominee ID Proof)$",
        ErrorMessage = "Invalid Document Type.")]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileBase64 { get; set; } = string.Empty;
}