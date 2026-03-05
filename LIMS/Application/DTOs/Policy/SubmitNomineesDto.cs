using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Customer submits all nominees at once
public class SubmitNomineesDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one nominee is required")]
    public List<AddNomineeDto> Nominees { get; set; } = new();
}