using Application.DTOs.Payment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IPolicyRepository _policyRepo;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IInvoiceRepository invoiceRepo,
        IPolicyRepository policyRepo)
    {
        _paymentRepo = paymentRepo;
        _invoiceRepo = invoiceRepo;
        _policyRepo = policyRepo;
    }

    public async Task<PaymentResponseDto> MakePaymentAsync(
        int customerId, MakePaymentDto dto)
    {
        var invoice = await _invoiceRepo.GetByIdWithDetailsAsync(dto.InvoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        // Verify this invoice belongs to the customer
        if (invoice.Policy.CustomerId != customerId)
            throw new UnauthorizedAccessException(
                "This invoice does not belong to you.");

        // Prevent double payment
        if (await _paymentRepo.InvoiceAlreadyPaidAsync(dto.InvoiceId))
            throw new InvalidOperationException("This invoice has already been paid.");

        // Cannot pay a cancelled invoice
        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("This invoice has been cancelled.");

        // Record payment
        var payment = new Payment
        {
            InvoiceId = dto.InvoiceId,
            PolicyId = invoice.PolicyId,
            AmountPaid = invoice.AmountDue,
            Status = PaymentStatus.Paid,
            PaymentMethod = dto.PaymentMethod,
            TransactionReference = dto.TransactionReference
                ?? Guid.NewGuid().ToString("N")[..12].ToUpper(),
            PaymentDate = DateTime.UtcNow
        };

        await _paymentRepo.CreateAsync(payment);

        // Update invoice status to Paid
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidOn = DateTime.UtcNow;
        await _invoiceRepo.UpdateAsync(invoice);

        // If policy was recently approved/verified, or in Grace/Suspended state, restore it to Active
        var policy = invoice.Policy;
        if (policy.Status == PolicyStatus.Approved ||
            policy.Status == PolicyStatus.DocumentsSubmitted ||
            policy.Status == PolicyStatus.Grace ||
            policy.Status == PolicyStatus.Suspended)
        {
            policy.Status = PolicyStatus.Active;
            if (policy.ActiveFrom == null)
            {
                policy.ActiveFrom = DateTime.UtcNow;
                policy.ActiveTo = DateTime.UtcNow.AddYears(policy.TenureYears);
            }
            await _policyRepo.UpdateAsync(policy);
        }

        return new PaymentResponseDto
        {
            PaymentId = payment.Id,
            AmountPaid = payment.AmountPaid,
            Status = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod,
            TransactionReference = payment.TransactionReference,
            PaymentDate = payment.PaymentDate
        };
    }

    public async Task<List<PaymentResponseDto>> GetPaymentHistoryAsync(int policyId)
    {
        var payments = await _paymentRepo.GetByPolicyIdAsync(policyId);
        return payments.Select(p => new PaymentResponseDto
        {
            PaymentId = p.Id,
            AmountPaid = p.AmountPaid,
            Status = p.Status.ToString(),
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            PaymentDate = p.PaymentDate
        }).ToList();
    }

    public async Task<List<PaymentResponseDto>> GetCustomerPaymentsAsync(int customerId)
    {
        var payments = await _paymentRepo.GetAllByCustomerAsync(customerId);
        return payments.Select(p => new PaymentResponseDto
        {
            PaymentId = p.Id,
            PolicyNumber = p.Policy?.PolicyNumber ?? "N/A",
            AmountPaid = p.AmountPaid,
            Status = p.Status.ToString(),
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            PaymentDate = p.PaymentDate
        }).ToList();
    }
}