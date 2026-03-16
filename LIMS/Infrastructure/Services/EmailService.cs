using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration config)
    {
        var apiKey = config["SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
        _fromEmail = config["SendGrid:SenderEmail"] ?? "notifications@nexalife.com";
        _fromName = config["SendGrid:SenderName"] ?? "NexaLife Insurance";
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        var from = new EmailAddress(_fromEmail, _fromName);
        var to = new EmailAddress(toEmail, toName);
        
        // Use the same content for plain text and HTML for simplicity, 
        // or you could strip HTML tags for the plain text version.
        var msg = MailHelper.CreateSingleEmail(from, to, subject, body.Replace("<br/>", "\n"), body);
        
        var response = await _client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"❌ Failed to send email to {toEmail}: {error}");
        }
        else
        {
            Console.WriteLine($"✅ Email sent successfully to {toEmail}");
        }
    }

    public async Task SendNomineePaymentEmail(
        string email,
        string nomineeName,
        string policyId,
        decimal amount,
        string transactionId,
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode)
    {
        var subject = "Claim Amount Successfully Transferred";
        var body = $@"
Dear {nomineeName},<br/><br/>
The claim amount for Policy ID {policyId} has been successfully transferred to your registered bank account.<br/><br/>
<strong>Payment Details:</strong><br/>
Policy ID: {policyId}<br/>
Amount Transferred: ₹{amount:N2}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
<strong>Bank Details:</strong><br/>
Bank Name: {bankName}<br/>
Account Holder Name: {accountHolderName}<br/>
Account Number: {accountNumber}<br/>
IFSC Code: {ifscCode}<br/><br/>
If you have any questions, please contact our support team.<br/><br/>
Thank you.";

        await SendEmailAsync(email, nomineeName, subject, body);
    }

    public async Task SendCustomerMaturityEmail(
        string email,
        string customerName,
        string policyId,
        decimal amount,
        string transactionId,
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode)
    {
        var subject = "Endowment Policy Maturity Amount Credited";
        var body = $@"
Dear {customerName},<br/><br/>
Your endowment policy has matured and the maturity amount has been successfully transferred to your bank account.<br/><br/>
<strong>Payment Details:</strong><br/>
Policy ID: {policyId}<br/>
Amount Credited: ₹{amount:N2}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
<strong>Bank Details:</strong><br/>
Bank Name: {bankName}<br/>
Account Holder Name: {accountHolderName}<br/>
Account Number: {accountNumber}<br/>
IFSC Code: {ifscCode}<br/><br/>
Thank you for choosing our insurance services.";

        await SendEmailAsync(email, customerName, subject, body);
    }

    public async Task SendAgentCommissionEmail(
        string email,
        string agentName,
        string policyId,
        decimal amount,
        string transactionId,
        DateTime transferDate,
        string bankName,
        string accountHolderName,
        string accountNumber,
        string ifscCode)
    {
        var subject = "Commission Payment Credited";
        var body = $@"
Dear {agentName},<br/><br/>
Your commission for Policy ID {policyId} has been successfully credited.<br/><br/>
<strong>Commission Details:</strong><br/>
Policy ID: {policyId}<br/>
Commission Amount: ₹{amount:N2}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
<strong>Bank Details:</strong><br/>
Bank Name: {bankName}<br/>
Account Holder Name: {accountHolderName}<br/>
Account Number: {accountNumber}<br/>
IFSC Code: {ifscCode}<br/><br/>
Thank you for your continued partnership with our insurance services.";

        await SendEmailAsync(email, agentName, subject, body);
    }

    public async Task SendPremiumPaymentEmail(
        string email,
        string customerName,
        string policyNumber,
        decimal amount,
        string transactionId,
        DateTime paymentDate,
        string paymentMethod)
    {
        var subject = $"Payment Receipt - Policy {policyNumber}";
        var body = $@"
Dear {customerName},<br/><br/>
Thank you for your premium payment. Your transaction has been successfully processed.<br/><br/>
<strong>Payment Details:</strong><br/>
Policy Number: {policyNumber}<br/>
Amount Paid: ₹{amount:N2}<br/>
Transaction ID: {transactionId}<br/>
Date & Time: {paymentDate:yyyy-MM-dd HH:mm:ss}<br/>
Payment Method: {paymentMethod}<br/><br/>
This email serves as an official receipt for your records.<br/><br/>
Thank you for choosing NexaLife Insurance.";

        await SendEmailAsync(email, customerName, subject, body);
    }
}


