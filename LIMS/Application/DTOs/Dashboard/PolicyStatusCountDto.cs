using Domain.Enums;

namespace Application.DTOs.Dashboard;

public class PolicyStatusCountDto
{
    public PolicyStatus Status { get; set; }
    public int Count { get; set; }
}