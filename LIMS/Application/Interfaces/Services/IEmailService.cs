namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
    
    Task SendNomineePaymentEmail(
        string email, 
        string nomineeName, 
        string policyId, 
        decimal amount, 
        string transactionId, 
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode);

    Task SendCustomerMaturityEmail(
        string email,
        string customerName,
        string policyId,
        decimal amount,
        string transactionId,
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode);

    Task SendPolicyActivationEmail(
        string email,
        string customerName,
        string policyNumber,
        string planName,
        decimal sumAssured,
        DateTime activeFrom,
        DateTime? activeTo);

    Task SendAgentCommissionEmail(
        string email,
        string agentName,
        string policyId,
        decimal amount,
        string transactionId,
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode);

    Task SendPremiumPaymentEmail(
        string email,
        string customerName,
        string policyNumber,
        decimal amount,
        string transactionId,
        DateTime paymentDate,
        string paymentMethod,
        byte[]? invoiceFile = null,
        string? invoiceFileName = null);
}


