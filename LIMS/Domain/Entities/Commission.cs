using Domain.Common;

namespace Domain.Entities;

public class Commission : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public int AgentId { get; set; }
    public User Agent { get; set; } = null!;

    public decimal PremiumAmount { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTime EarnedOn { get; set; } = DateTime.UtcNow;
}