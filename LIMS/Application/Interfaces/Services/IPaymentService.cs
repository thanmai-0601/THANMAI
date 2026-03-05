using Application.DTOs.Payment;

namespace Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> MakePaymentAsync(int customerId, MakePaymentDto dto);
    Task<List<PaymentResponseDto>> GetPaymentHistoryAsync(int policyId);
    Task<List<PaymentResponseDto>> GetCustomerPaymentsAsync(int customerId);
}