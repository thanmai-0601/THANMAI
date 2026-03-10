using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Policy;

// Customer submits all nominees at once
public class SubmitNomineesDto
{
    [Required]
    public AddNomineeDto Nominee { get; set; } = new();
}