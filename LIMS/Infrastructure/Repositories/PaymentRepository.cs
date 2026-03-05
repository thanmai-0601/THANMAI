using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<Payment>> GetByPolicyIdAsync(int policyId)
        => await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.PolicyId == policyId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<bool> InvoiceAlreadyPaidAsync(int invoiceId)
        => await _context.Payments
            .AnyAsync(p =>
                p.InvoiceId == invoiceId &&
                p.Status == Domain.Enums.PaymentStatus.Paid);
    public async Task<decimal> GetTotalPremiumCollectedAsync()
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.AmountPaid);
    }

    public async Task<decimal> GetTotalPaidByCustomerAsync(int customerId)
    {
        return await _context.Payments
            .Where(p =>
                p.Policy.CustomerId == customerId &&
                p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.AmountPaid);
    }
    public async Task<List<MonthlyRevenueDto>> GetLast12MonthsRevenueAsync()
    {
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);

        return await _context.Payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaymentDate >= twelveMonthsAgo)
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                PremiumCollected = g.Sum(p => p.AmountPaid),
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1)
                    .ToString("MMM yyyy")
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();
    }
 
    public async Task<List<Payment>> GetRecentByCustomerAsync(int customerId, int count)
    {
        return await _context.Payments
            .Include(p => p.Policy)
            .Include(p => p.Invoice)
            .Where(p => p.Policy.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetAllByCustomerAsync(int customerId)
    {
        return await _context.Payments
            .Include(p => p.Policy)
            .Include(p => p.Invoice)
            .Where(p => p.Policy.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}