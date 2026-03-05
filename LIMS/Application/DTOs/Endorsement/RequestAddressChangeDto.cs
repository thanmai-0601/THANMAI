using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Endorsement;

public class RequestAddressChangeDto
{
    [Required]
    public int PolicyId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10,
        ErrorMessage = "Please provide a complete address")]
    public string NewAddress { get; set; } = string.Empty;
}