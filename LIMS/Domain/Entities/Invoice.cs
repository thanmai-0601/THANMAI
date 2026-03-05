using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Invoice : BaseEntity
{
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public string InvoiceNumber { get; set; } = string.Empty;
    public int PeriodYear { get; set; }           // which year this invoice covers e.g. 2024
    public int PeriodNumber { get; set; }         // e.g. quarter 1, month 3
    public PaymentFrequency Frequency { get; set; }

    public decimal AmountDue { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? GraceEndDate { get; set; }   // DueDate + 30 days
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Generated;

    public DateTime? PaidOn { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public Payment? Payment { get; set; }
}