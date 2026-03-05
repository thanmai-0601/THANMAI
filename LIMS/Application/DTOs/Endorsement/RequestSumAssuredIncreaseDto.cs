using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Endorsement;

public class RequestSumAssuredIncreaseDto
{
    [Required]
    public int PolicyId { get; set; }

    [Required]
    [Range(100000, double.MaxValue,
        ErrorMessage = "New sum assured must be at least 1,00,000")]
    public decimal NewSumAssured { get; set; }
}