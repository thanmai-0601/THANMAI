using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment;

public class MakePaymentDto
{
    [Required]
    public int InvoiceId { get; set; }

    [Required]
    [RegularExpression("^(UPI|Card|NetBanking)$",
        ErrorMessage = "Payment method must be UPI, Card, or NetBanking")]
    public string PaymentMethod { get; set; } = string.Empty;

    // Simulated transaction reference — in real app payment gateway provides this
    public string? TransactionReference { get; set; }
}