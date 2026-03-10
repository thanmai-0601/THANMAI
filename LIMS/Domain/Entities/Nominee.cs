using Domain.Common;
using Domain.Entities;

namespace Domain.Entities;

public class Nominee : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty; // e.g. Aadhar/PAN number provided by customer
    public string Email { get; set; } = string.Empty;     // for settlement confirmation

    // All nominee percentages for a policy must add up to 100
    public decimal AllocationPercentage { get; set; }
}