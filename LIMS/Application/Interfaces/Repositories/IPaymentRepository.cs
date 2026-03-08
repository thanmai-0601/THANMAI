using Application.DTOs.Dashboard;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);
    Task<List<Payment>> GetByPolicyIdAsync(int policyId);
    Task<bool> InvoiceAlreadyPaidAsync(int invoiceId);
    Task<decimal> GetTotalPremiumCollectedAsync();
    Task<List<MonthlyRevenueDto>> GetLast12MonthsRevenueAsync();
    Task<List<MonthlyRevenueDto>> GetRevenueByYearAsync(int year);
    Task<decimal> GetTotalPaidByCustomerAsync(int customerId);
    Task<List<Payment>> GetRecentByCustomerAsync(int customerId, int count);
    Task<List<Payment>> GetAllByCustomerAsync(int customerId);
    Task<List<Payment>> GetRecentAsync(int count);
}