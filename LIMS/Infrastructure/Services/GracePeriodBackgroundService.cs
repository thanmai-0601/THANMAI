using Application.Interfaces.Repositories;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class GracePeriodBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GracePeriodBackgroundService> _logger;

    // Run once every 24 hours
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public GracePeriodBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<GracePeriodBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Grace Period Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOverdueInvoicesAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessOverdueInvoicesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var policyRepo = scope.ServiceProvider.GetRequiredService<IPolicyRepository>();

        try
        {
            // ── Step 1: Pending invoices past due date → move to Grace ──────
            var overdueInvoices = await invoiceRepo.GetOverdueInvoicesAsync();

            foreach (var invoice in overdueInvoices)
            {
                invoice.Status = InvoiceStatus.Grace;
                await invoiceRepo.UpdateAsync(invoice);

                // Move policy to Grace status
                var policy = await policyRepo.GetByIdAsync(invoice.PolicyId);
                if (policy != null && policy.Status == PolicyStatus.Active)
                {
                    policy.Status = PolicyStatus.Grace;
                    await policyRepo.UpdateAsync(policy);
                    _logger.LogInformation(
                        "Policy {PolicyId} moved to Grace period.", policy.Id);
                }
            }

            // ── Step 2: Grace invoices past grace end date → Suspended ───────
            var graceExpiredInvoices = await invoiceRepo.GetGraceInvoicesAsync();

            foreach (var invoice in graceExpiredInvoices)
            {
                invoice.Status = InvoiceStatus.Overdue;
                await invoiceRepo.UpdateAsync(invoice);

                var policy = await policyRepo.GetByIdAsync(invoice.PolicyId);
                if (policy == null) continue;

                // First expiry → Suspended, second → Lapsed
                if (policy.Status == PolicyStatus.Grace)
                {
                    policy.Status = PolicyStatus.Suspended;
                    await policyRepo.UpdateAsync(policy);
                    _logger.LogInformation(
                        "Policy {PolicyId} Suspended.", policy.Id);
                }
                else if (policy.Status == PolicyStatus.Suspended)
                {
                    policy.Status = PolicyStatus.Lapsed;
                    await policyRepo.UpdateAsync(policy);
                    _logger.LogInformation(
                        "Policy {PolicyId} Lapsed.", policy.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Grace Period Background Service.");
        }
    }
}