using Domain.Enums;

namespace Application.DTOs.Dashboard;

public class ClaimStatusCountDto
{
    public ClaimStatus Status { get; set; }
    public int Count { get; set; }
}