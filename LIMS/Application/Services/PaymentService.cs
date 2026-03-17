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
    private readonly ICommissionRepository _commissionRepo;
    private readonly IEmailService _emailService;


    public PaymentService(
        IPaymentRepository paymentRepo,
        IInvoiceRepository invoiceRepo,
        IPolicyRepository policyRepo,
        ICommissionRepository commissionRepo,
        IEmailService emailService)
    {
        _paymentRepo = paymentRepo;
        _invoiceRepo = invoiceRepo;
        _policyRepo = policyRepo;
        _commissionRepo = commissionRepo;
        _emailService = emailService;
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
        var isInitialActivation = (policy.Status == PolicyStatus.Approved || policy.Status == PolicyStatus.DocumentsSubmitted);
        // If this is the FIRST premium payment, move to Active
        if (policy.Status == PolicyStatus.Approved ||
            policy.Status == PolicyStatus.DocumentsSubmitted ||
            policy.Status == PolicyStatus.Grace ||
            policy.Status == PolicyStatus.Suspended)
        {
            policy.Status = PolicyStatus.Active;

            if (policy.ActiveFrom == null)
            {
                policy.ActiveFrom = DateTime.UtcNow;
                if (policy.InsurancePlan?.PlanType == PlanType.WholeLife)
                {
                    policy.ActiveTo = null; // Lifetime coverage
                }
                else
                {
                    policy.ActiveTo = policy.ActiveFrom.Value.AddYears(policy.TenureYears);
                }
            }

            await _policyRepo.UpdateAsync(policy);

            // Send Activation Email to Customer (Ensure Customer and Plan are loaded)
            if (isInitialActivation)
            {
                var customer = policy.Customer;
                var plan = policy.InsurancePlan;

                if (customer != null && plan != null)
                {
                    await _emailService.SendPolicyActivationEmail(
                        customer.Email,
                        customer.FullName,
                        policy.PolicyNumber,
                        plan.PlanName,
                        policy.SumAssured,
                        policy.ActiveFrom.Value,
                        policy.ActiveTo
                    );
                }
            }
        }

        // Update commission status and Notify Agent
        var commission = await _commissionRepo.GetByPolicyIdAsync(invoice.PolicyId);
        if (commission != null && commission.Status == CommissionStatus.Pending)
        {
            commission.Status = CommissionStatus.Earned;
            commission.EarnedOn = DateTime.UtcNow;
            await _commissionRepo.UpdateAsync(commission);

            // Send Email to Agent for earned commission
            // Ensure Agent details are available
            var agent = commission.Agent ?? policy.Agent;
            if (agent != null && !string.IsNullOrEmpty(agent.Email))
            {
                await _emailService.SendAgentCommissionEmail(
                    agent.Email,
                    agent.FullName,
                    policy.PolicyNumber,
                    commission.CommissionAmount,
                    payment.TransactionReference,
                    payment.PaymentDate,
                    agent.BankAccountName ?? "Registered Agent Bank",
                    agent.FullName,
                    agent.BankAccountNumber ?? "XXXX",
                    agent.BankIfscCode ?? ""
                );
            }
        }

        // Send Email Receipt to Customer
        if (invoice.Policy?.Customer != null)
        {
            var htmlInvoice = $@"
<html>
<head>
  <style>
    body {{ font-family: 'Inter', sans-serif; padding: 40px; color: #1e293b; line-height: 1.5; }}
    .header {{ border-bottom: 2px solid #e2e8f0; padding-bottom: 20px; margin-bottom: 30px; display: flex; justify-content: space-between; align-items: flex-end; }}
    .logo {{ font-size: 24px; font-weight: 800; color: #f97316; }}
    .invoice-label {{ font-size: 32px; font-weight: 900; text-transform: uppercase; letter-spacing: -0.025em; float: right; }}
    .grid {{ display: flex; justify-content: space-between; margin-bottom: 40px; }}
    .label {{ font-size: 10px; font-weight: 800; text-transform: uppercase; color: #64748b; margin-bottom: 4px; }}
    .value {{ font-size: 14px; font-weight: 600; margin-bottom: 10px; }}
    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
    th {{ text-align: left; padding: 12px; border-bottom: 1px solid #e2e8f0; font-size: 10px; font-weight: 800; text-transform: uppercase; color: #64748b; }}
    td {{ padding: 16px 12px; border-bottom: 1px solid #f1f5f9; font-size: 14px; }}
    .total-row td {{ border-top: 2px solid #e2e8f0; font-weight: 800; font-size: 18px; }}
    .footer {{ margin-top: 60px; text-align: center; font-size: 12px; color: #94a3b8; }}
    .info-box {{ margin-top: 40px; padding: 20px; background: #f8fafc; border-radius: 12px; font-size: 12px; border: 1px solid #e2e8f0; }}
  </style>
</head>
<body>
  <div class=""header"">
    <div style=""display: inline-block"">
      <div class=""logo"">NexaLife Core</div>
      <div style=""font-size: 12px; color: #64748b;"">Life Insurance Management System</div>
    </div>
    <div style=""display: inline-block"">
      <div class=""invoice-label"">Invoice</div>
    </div>
  </div>

  <div class=""grid"">
    <div style=""display: inline-block; width: 50%"">
      <div class=""label"">Billed To</div>
      <div class=""value"">{invoice.Policy.Customer.FullName}</div>
      <div class=""value"">Policy: {invoice.Policy.PolicyNumber}</div>
    </div>
    <div style=""display: inline-block; width: 49%; text-align: right;"">
      <div class=""label"">Invoice Number</div>
      <div class=""value"">{invoice.InvoiceNumber}</div>
      <div class=""label"">Date Paid</div>
      <div class=""value"">{payment.PaymentDate:yyyy-MM-dd}</div>
      <div class=""label"">Transaction ID</div>
      <div class=""value"">{payment.TransactionReference}</div>
    </div>
  </div>

  <table>
    <thead>
      <tr>
        <th>Description</th>
        <th style=""text-align: right;"">Amount</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Insurance Premium - Year {invoice.PeriodYear} / Installment #{invoice.PeriodNumber}</td>
        <td style=""text-align: right;"">₹{invoice.AmountDue:N2}</td>
      </tr>
      <tr class=""total-row"">
        <td>Total Amount Paid</td>
        <td style=""text-align: right;"">₹{payment.AmountPaid:N2}</td>
      </tr>
    </tbody>
  </table>

  <div class=""info-box"">
    <div class=""label"">Payment Method</div>
    <div class=""value"">{payment.PaymentMethod}</div>
    <div class=""label"">Status</div>
    <div class=""value"" style=""color: #10b981;"">PAID SUCCESSFULLY</div>
  </div>

  <div class=""footer"">
    <p>This is a computer-generated document. No signature is required.</p>
    <p>&copy; {DateTime.UtcNow.Year} NexaLife Core. All rights reserved.</p>
  </div>
</body>
</html>";

            var fileBytes = System.Text.Encoding.UTF8.GetBytes(htmlInvoice);

            await _emailService.SendPremiumPaymentEmail(
                invoice.Policy.Customer.Email,
                invoice.Policy.Customer.FullName,
                invoice.Policy.PolicyNumber,
                payment.AmountPaid,
                payment.TransactionReference,
                payment.PaymentDate,
                payment.PaymentMethod.ToString(),
                fileBytes,
                $"{invoice.InvoiceNumber}.html"
            );
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