namespace Domain.Enums;

public enum InvoiceStatus
{
    Generated = 1,
    Pending = 2,
    Paid = 3,
    Overdue = 4,
    Grace = 5,
    Cancelled = 6
}