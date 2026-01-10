using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using EventPro.Kernal.StaticFiles;
using EventPro.Services.PinnacleService.Interface;


namespace EventPro.Services.NotificationService.Implementation
{
    public class NotificationService : IEmailSender  /*INotificationService*/
    {
        private readonly IPinnacleService _pinacleService;
        private readonly IConfiguration _configuration;
        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("bookstore(EventPro Notification)"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using (var emailClient = new SmtpClient())
            {
                try
                {
                    emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    emailClient.Authenticate(StaticPinnacleBalance.EventProNotificationEmail, StaticPinnacleBalance.EventProNotificationPassword);
                    emailClient.Send(emailToSend);
                    //emailClient.Disconnect(true);
                }
                catch { }
            }
            return Task.CompletedTask;
        }

    }
}
