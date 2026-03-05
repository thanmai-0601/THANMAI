using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int invoiceId);
    Task<Invoice?> GetByIdWithDetailsAsync(int invoiceId);
    Task<List<Invoice>> GetByPolicyIdAsync(int policyId);
    Task<List<Invoice>> GetOverdueInvoicesAsync();    // for background job
    Task<List<Invoice>> GetGraceInvoicesAsync();      // for background job
    Task<string> GenerateInvoiceNumberAsync();
    Task CreateRangeAsync(List<Invoice> invoices);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task<int> GetOverdueCountByCustomerAsync(int customerId);
    Task<int> GetUpcomingCountByCustomerAsync(int customerId);
    Task<decimal> GetOutstandingAmountByCustomerAsync(int customerId);
}