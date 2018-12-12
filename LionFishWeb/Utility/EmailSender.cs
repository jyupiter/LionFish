using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LionFishWeb.Utility
{
    public class EmailSender
    {
        public static async Task Activate(string t, string key, string sub)
        {
            var apiKey = Environment.GetEnvironmentVariable("LIONFISH");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("fnytxt@gmail.com", "LionFish");
            var subject = sub;
            var to = new EmailAddress(t, "User");
            var plainTextContent = "Enter this code in the confirmation page: " + key;
            var htmlContent = "Enter this code in the confirmation page: <b>" + key + "</b>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            Debug.WriteLine("\n " + Environment.GetEnvironmentVariable("LIONFISH") + " \n");
            await client.SendEmailAsync(msg);
        }
    }
}