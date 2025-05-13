using BookLibraryStore.Data.Service.IService;
using BookLibraryStore.Models.ServiceModel;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookLibraryStore.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly ICustomEmailService _emailService;

        public EmailSender(ICustomEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Logic to send email
            await _emailService.SendEmailAsync(new MailRequest
            {
                ToEmail = email,
                Subject = subject,
                Body = htmlMessage
            });
        }
    }
}
