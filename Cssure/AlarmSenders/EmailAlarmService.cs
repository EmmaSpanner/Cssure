using System.Net;
using System.Net.Mail;

namespace Cssure.AlarmSenders
{
    public class EmailAlarmService : IEmailSender
    {
        public Task SendEmailAsync(string[] email, string subject, string message)
        {
            var mail = "TemoTestMail@gmail.com";
            var pw = "vkjrlfpkwkzhubaf";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            var mailMessage = new MailMessage(from: mail,
                                to: email.First(), //First since we only type one email address
                                subject,
                                message);

            mailMessage.Headers.Add("Importance", "High");
            return client.SendMailAsync(mailMessage);
        }
    }
}
