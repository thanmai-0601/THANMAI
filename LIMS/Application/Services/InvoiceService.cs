using Application.DTOs.Payment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IPolicyRepository _policyRepo;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        IPolicyRepository policyRepo)
    {
        _invoiceRepo = invoiceRepo;
        _policyRepo = policyRepo;
    }

    // Generates the full invoice schedule when policy activates
    public async Task GenerateScheduleAsync(int policyId, PaymentFrequency frequency)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        var annualPremium = policy.PremiumAmount!.Value;
        var startDate = policy.ActiveFrom ?? DateTime.UtcNow;
        var invoices = new List<Invoice>();

        // Calculate installment amount and count based on frequency
        var (installmentAmount, installmentsPerYear) = frequency switch
        {
            PaymentFrequency.Annual => (annualPremium, 1),
            PaymentFrequency.Quarterly => (Math.Round(annualPremium / 4, 2), 4),
            PaymentFrequency.Monthly => (Math.Round(annualPremium / 12, 2), 12),
            _ => throw new InvalidOperationException("Invalid payment frequency.")
        };

        var totalInstallments = installmentsPerYear * policy.TenureYears;
        var startInvoiceNumber = await _invoiceRepo.GenerateInvoiceNumberAsync();
        var prefix = startInvoiceNumber.Substring(0, 8); // e.g. "INV-2024"
        var baseNumber = int.Parse(startInvoiceNumber.Substring(8)); // e.g. "0001" -> 1

        // Generate one invoice per installment across the full tenure
        for (int i = 0; i < totalInstallments; i++)
        {
            var dueDate = frequency switch
            {
                PaymentFrequency.Annual => startDate.AddYears(i),
                PaymentFrequency.Quarterly => startDate.AddMonths(i * 3),
                PaymentFrequency.Monthly => startDate.AddMonths(i),
                _ => throw new InvalidOperationException()
            };

            var invoiceNumber = $"{prefix}{(baseNumber + i):D4}";

            invoices.Add(new Invoice
            {
                PolicyId = policyId,
                InvoiceNumber = invoiceNumber,
                PeriodYear = dueDate.Year,
                PeriodNumber = i + 1,
                Frequency = frequency,
                AmountDue = installmentAmount,
                DueDate = dueDate,
                GraceEndDate = dueDate.AddDays(30),  // 30-day grace period
                Status = i == 0
                    ? InvoiceStatus.Pending   // first invoice due immediately
                    : InvoiceStatus.Generated // rest are future
            });
        }

        await _invoiceRepo.CreateRangeAsync(invoices);
    }

    public async Task<PaymentScheduleDto> GetScheduleAsync(int policyId)
    {
        var policy = await _policyRepo.GetByIdWithDetailsAsync(policyId)
            ?? throw new KeyNotFoundException("Policy not found.");

        var invoices = await _invoiceRepo.GetByPolicyIdAsync(policyId);

        if (!invoices.Any())
            throw new InvalidOperationException("No invoice schedule found for this policy.");

        var first = invoices.First();

        return new PaymentScheduleDto
        {
            PolicyNumber = policy.PolicyNumber,
            Frequency = first.Frequency.ToString(),
            InstallmentAmount = first.AmountDue,
            TotalInstallments = invoices.Count,
            TotalPayable = invoices.Sum(i => i.AmountDue),
            Invoices = invoices.Select(MapToDto).ToList()
        };
    }

    public async Task<List<InvoiceResponseDto>> GetPolicyInvoicesAsync(int policyId)
    {
        var invoices = await _invoiceRepo.GetByPolicyIdAsync(policyId);
        return invoices.Select(MapToDto).ToList();
    }

    public async Task<InvoiceResponseDto> GetInvoiceAsync(int invoiceId)
    {
        var invoice = await _invoiceRepo.GetByIdWithDetailsAsync(invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");
        return MapToDto(invoice);
    }

    private static InvoiceResponseDto MapToDto(Invoice i) => new()
    {
        InvoiceId = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        PolicyNumber = i.Policy?.PolicyNumber ?? string.Empty,
        PeriodYear = i.PeriodYear,
        PeriodNumber = i.PeriodNumber,
        Frequency = i.Frequency.ToString(),
        AmountDue = i.AmountDue,
        DueDate = i.DueDate,
        GraceEndDate = i.GraceEndDate,
        Status = i.Status.ToString(),
        PaidOn = i.PaidOn,
        Remarks = i.Remarks,
        Payment = i.Payment == null ? null : new PaymentResponseDto
        {
            PaymentId = i.Payment.Id,
            AmountPaid = i.Payment.AmountPaid,
            Status = i.Payment.Status.ToString(),
            PaymentMethod = i.Payment.PaymentMethod,
            TransactionReference = i.Payment.TransactionReference,
            PaymentDate = i.Payment.PaymentDate
        }
    };
}