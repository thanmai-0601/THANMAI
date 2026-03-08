namespace Application.DTOs.Payment;

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
}