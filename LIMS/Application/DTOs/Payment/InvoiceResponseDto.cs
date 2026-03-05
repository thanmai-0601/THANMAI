namespace Application.DTOs.Payment;

public class InvoiceResponseDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public int PeriodNumber { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? GraceEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidOn { get; set; }
    public string? Remarks { get; set; }
    public PaymentResponseDto? Payment { get; set; }
}