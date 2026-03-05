using Application.DTOs.Payment;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IInvoiceService
{
    // Called automatically when policy becomes Active
    Task GenerateScheduleAsync(int policyId, PaymentFrequency frequency);

    Task<PaymentScheduleDto> GetScheduleAsync(int policyId);
    Task<List<InvoiceResponseDto>> GetPolicyInvoicesAsync(int policyId);
    Task<InvoiceResponseDto> GetInvoiceAsync(int invoiceId);
}