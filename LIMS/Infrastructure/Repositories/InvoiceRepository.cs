using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int invoiceId)
        => await _context.Invoices.FindAsync(invoiceId);

    public async Task<Invoice?> GetByIdWithDetailsAsync(int invoiceId)
        => await _context.Invoices
            .Include(i => i.Policy)
                .ThenInclude(p => p.InsurancePlan)
            .Include(i => i.Policy)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Policy)
                .ThenInclude(p => p.Agent)
            .Include(i => i.Payment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

    public async Task<List<Invoice>> GetByPolicyIdAsync(int policyId)
        => await _context.Invoices
            .Include(i => i.Payment)
            .Where(i => i.PolicyId == policyId)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

    // Invoices past due date, not paid, not already in grace
    public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        => await _context.Invoices
            .Include(i => i.Policy)
            .Where(i =>
                i.Status == InvoiceStatus.Pending &&
                i.DueDate < DateTime.UtcNow)
            .ToListAsync();

    // Invoices in grace period that have now exceeded grace end date
    public async Task<List<Invoice>> GetGraceInvoicesAsync()
        => await _context.Invoices
            .Include(i => i.Policy)
            .Where(i =>
                i.Status == InvoiceStatus.Grace &&
                i.GraceEndDate < DateTime.UtcNow)
            .ToListAsync();

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}";

        var last = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (last != null)
        {
            var lastNum = int.Parse(last.InvoiceNumber.Split('-')[1][4..]);
            nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task CreateRangeAsync(List<Invoice> invoices)
    {
        _context.Invoices.AddRange(invoices);
        await _context.SaveChangesAsync();
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
    {
        invoice.UpdatedAt = DateTime.UtcNow;
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
    public async Task<decimal> GetTotalPaidByCustomerAsync(int customerId)
    {
        return await _context.Payments
            .Where(p =>
                p.Policy.CustomerId == customerId &&
                p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.AmountPaid);
    }
    public async Task<int> GetUpcomingCountByCustomerAsync(int customerId)
    {
        var future = DateTime.UtcNow.AddDays(30);

        return await _context.Invoices
            .CountAsync(i =>
                i.Policy.CustomerId == customerId &&
                i.Status == InvoiceStatus.Pending &&
                i.DueDate <= future);
    }
    public async Task<decimal> GetOutstandingAmountByCustomerAsync(int customerId)
    {
        return await _context.Invoices
            .Where(i =>
                i.Policy.CustomerId == customerId &&
                i.Status != InvoiceStatus.Paid &&
                i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.AmountDue);
    }

    public async Task<int> GetOverdueCountByCustomerAsync(int customerId)
    {
        return await _context.Invoices
            .CountAsync(i =>
                i.Policy.CustomerId == customerId &&
                (i.Status == InvoiceStatus.Overdue ||
                 i.Status == InvoiceStatus.Grace));
    }
}