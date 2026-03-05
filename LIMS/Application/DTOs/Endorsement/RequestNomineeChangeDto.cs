using System.ComponentModel.DataAnnotations;
using Application.DTOs.Policy;

namespace Application.DTOs.Endorsement;

public class RequestNomineeChangeDto
{
    [Required]
    public int PolicyId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one nominee is required")]
    public List<AddNomineeDto> NewNominees { get; set; } = new();
}