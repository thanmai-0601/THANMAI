namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendSettlementConfirmationAsync(
        string nomineeEmail,
        string nomineeName,
        string policyNumber,
        string claimNumber,
        decimal settledAmount,
        string bankAccountName,
        string bankAccountNumber,
        string transferReference);
}
