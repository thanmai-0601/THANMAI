namespace Application.DTOs.Payment;

// Full EMI schedule shown to customer
public class PaymentScheduleDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public decimal InstallmentAmount { get; set; }
    public int TotalInstallments { get; set; }
    public decimal TotalPayable { get; set; }
    public List<InvoiceResponseDto> Invoices { get; set; } = new();
}