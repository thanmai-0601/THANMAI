namespace Application.DTOs.Policy;

public class NomineeResponseDto
{
    public int NomineeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
}