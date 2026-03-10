using System.Net;
using System.Net.Mail;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendSettlementConfirmationAsync(
        string nomineeEmail,
        string nomineeName,
        string policyNumber,
        string claimNumber,
        decimal settledAmount,
        string bankAccountName,
        string bankAccountNumber,
        string transferReference)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var senderEmail = _config["Email:SenderEmail"] ?? "";
        var senderPassword = _config["Email:SenderPassword"] ?? "";
        var senderName = _config["Email:SenderName"] ?? "NexaLife Insurance";

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
        {
            Console.WriteLine($"⚠️ Email not configured. Skipping email to {nomineeEmail}");
            Console.WriteLine($"   → Claim {claimNumber} settled for ₹{settledAmount:N0} to {bankAccountName}");
            return;
        }

        var maskedAccount = bankAccountNumber.Length > 4
            ? "XXXX-XXXX-" + bankAccountNumber[^4..]
            : bankAccountNumber;

        var subject = $"NexaLife Settlement Confirmation — {claimNumber}";

        var body = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #1e3a5f, #3b82f6); padding: 30px; border-radius: 16px 16px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 24px;'>🏦 NexaLife Insurance</h1>
        <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0;'>Claim Settlement Confirmation</p>
    </div>
    
    <div style='padding: 30px; background: #f9fafb; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 16px 16px;'>
        <p style='font-size: 16px;'>Dear <strong>{nomineeName}</strong>,</p>
        
        <p>We are pleased to inform you that the insurance claim associated with your policy has been <strong style='color: #059669;'>successfully settled</strong>.</p>
        
        <table style='width: 100%; border-collapse: collapse; margin: 24px 0; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
            <tr style='border-bottom: 1px solid #f3f4f6;'>
                <td style='padding: 14px 20px; color: #6b7280; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;'>Policy Number</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right;'>{policyNumber}</td>
            </tr>
            <tr style='border-bottom: 1px solid #f3f4f6;'>
                <td style='padding: 14px 20px; color: #6b7280; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;'>Claim Number</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right;'>{claimNumber}</td>
            </tr>
            <tr style='border-bottom: 1px solid #f3f4f6; background: #ecfdf5;'>
                <td style='padding: 14px 20px; color: #059669; font-size: 13px; text-transform: uppercase; letter-spacing: 1px; font-weight: bold;'>Amount Settled</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right; color: #059669; font-size: 18px;'>₹{settledAmount:N0}</td>
            </tr>
            <tr style='border-bottom: 1px solid #f3f4f6;'>
                <td style='padding: 14px 20px; color: #6b7280; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;'>Paid To</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right;'>{bankAccountName}</td>
            </tr>
            <tr style='border-bottom: 1px solid #f3f4f6;'>
                <td style='padding: 14px 20px; color: #6b7280; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;'>Account</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right; font-family: monospace;'>{maskedAccount}</td>
            </tr>
            <tr>
                <td style='padding: 14px 20px; color: #6b7280; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;'>Transfer Ref</td>
                <td style='padding: 14px 20px; font-weight: bold; text-align: right; font-family: monospace; color: #3b82f6;'>{transferReference}</td>
            </tr>
        </table>
        
        <p style='font-size: 13px; color: #6b7280;'>If you have any questions regarding this settlement, please contact our support team.</p>
        
        <p style='font-size: 13px; color: #9ca3af; margin-top: 30px; border-top: 1px solid #e5e7eb; padding-top: 20px;'>
            This is an automated email from NexaLife Insurance. Please do not reply to this email.
        </p>
    </div>
</body>
</html>";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(nomineeEmail, nomineeName));

        try
        {
            await client.SendMailAsync(message);
            Console.WriteLine($"✅ Settlement email sent to {nomineeName} ({nomineeEmail})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email to {nomineeEmail}: {ex.Message}");
        }
    }
}
