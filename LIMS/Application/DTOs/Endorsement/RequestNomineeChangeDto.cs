using System.ComponentModel.DataAnnotations;
using Application.DTOs.Policy;

namespace Application.DTOs.Endorsement;

public class RequestNomineeChangeDto
{
    [Required]
    public int PolicyId { get; set; }

    [Required]
    public AddNomineeDto NewNominee { get; set; } = new();
}