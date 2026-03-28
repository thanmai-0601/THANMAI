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

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        var from = new EmailAddress(_fromEmail, _fromName);
        var to = new EmailAddress(toEmail, toName);
        
        // Strip HTML tags for plain text version
        var plainTextContent = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", string.Empty);
        var htmlContent = body;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
        if (attachment != null && !string.IsNullOrEmpty(attachmentName))
        {
            var base64File = Convert.ToBase64String(attachment);
            msg.AddAttachment(attachmentName, base64File);
        }

        try
        {
            var response = await _client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {error}");
                Console.WriteLine("⚠️ Note: SendGrid may block @gmail.com Sender Emails due to DMARC, or your API key may be revoked.");
            }
            else
            {
                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SendGrid Exception during email dispatch to {toEmail}: {ex.Message}");
        }
    }

    public async Task SendPolicyActivationEmail(
        string email,
        string customerName,
        string policyNumber,
        string planName,
        decimal sumAssured,
        DateTime activeFrom,
        DateTime? activeTo)
    {
        var subject = $"Welcome to NexaLife - Policy {policyNumber} Activated!";
        var endDateStr = activeTo.HasValue ? activeTo.Value.ToString("yyyy-MM-dd") : "Lifetime";
        
        var body = $@"
Dear {customerName},<br/><br/>
Congratulations! Your premium payment has been successfully processed, and your insurance policy is now <b>Active</b>.<br/><br/>
<b>Policy Details:</b><br/>
Policy Number: {policyNumber}<br/>
Plan Name: {planName}<br/>
Coverage Amount (Sum Assured): ₹{sumAssured:N0}<br/>
Coverage Start Date: {activeFrom:yyyy-MM-dd}<br/>
Coverage End Date: {endDateStr}<br/><br/>
Thank you for entrusting NexaLife Insurance with your coverage. You can download your policy documents and track your schedule anytime from your customer dashboard.<br/><br/>
If you have any questions, please contact your assigned agent or our support team.<br/><br/>
Best regards,<br/>
The NexaLife Team";

        await SendEmailAsync(email, customerName, subject, body);
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
Payment Details:<br/>
Policy ID: {policyId}<br/>
Amount Transferred: ₹{amount:0.00}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
Bank Details:<br/>
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
Payment Details:<br/>
Policy ID: {policyId}<br/>
Amount Credited: ₹{amount:0.00}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
Bank Details:<br/>
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
Commission Details:<br/>
Policy ID: {policyId}<br/>
Commission Amount: ₹{amount:0.00}<br/>
Transaction ID: {transactionId}<br/>
Transfer Date: {transferDate:yyyy-MM-dd}<br/><br/>
Bank Details:<br/>
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
        string paymentMethod,
        byte[]? invoiceFile = null,
        string? invoiceFileName = null)
    {
        var subject = $"Payment Receipt - Policy {policyNumber}";
        var body = $@"
Dear {customerName},<br/><br/>
Thank you for your premium payment. Your transaction has been successfully processed.<br/><br/>
Payment Details:<br/>
Policy Number: {policyNumber}<br/>
Amount Paid: ₹{amount:0.00}<br/>
Transaction ID: {transactionId}<br/>
Date & Time: {paymentDate:yyyy-MM-dd HH:mm:ss}<br/>
Payment Method: {paymentMethod}<br/><br/>
This email serves as an official receipt for your records.<br/><br/>
Thank you for choosing NexaLife Insurance.";

        await SendEmailAsync(email, customerName, subject, body, invoiceFile, invoiceFileName);
    }

    public async Task SendWelcomeEmail(string email, string customerName)
    {
        var subject = "Welcome to NexaLife Insurance!";
        var body = $@"
Dear {customerName},<br/><br/>
Welcome to NexaLife! We are thrilled to have you with us.<br/><br/>
Your account has been successfully created. You can now browse our insurance plans, request new policies, and manage your profile directly from your dashboard.<br/><br/>
If you have any questions, our support team is always here to help.<br/><br/>
Best regards,<br/>
The NexaLife Team";

        await SendEmailAsync(email, customerName, subject, body);
    }
}


