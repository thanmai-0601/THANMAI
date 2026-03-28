using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace TestSendGrid
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("d:\\THANMAI\\LIMS\\API\\appsettings.json").AddEnvironmentVariables().Build();
            var apiKey = config["SendGrid:ApiKey"];
            var fromEmail = config["SendGrid:SenderEmail"] ?? "notifications@nexalife.com";
            var fromName = config["SendGrid:SenderName"] ?? "NexaLife";
            
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(fromEmail, "Self");
            var msg = MailHelper.CreateSingleEmail(from, to, "Test", "Test", "<b>Test</b>");
            
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Body: " + await response.Body.ReadAsStringAsync());
        }
    }
}
