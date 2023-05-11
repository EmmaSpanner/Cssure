﻿using System.Net;
using System.Net.Mail;

namespace Cssure.AlarmSenders
{
    public class EmailAlarmService : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = "TemoTestMail@gmail.com";
            //var pw = "!Temo123";
            var pw = "vkjrlfpkwkzhubaf";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            var mailMessage = new MailMessage(from: mail,
                                to: email,
                                subject,
                                message);
            //mailMessage.Priority = MailPriority.High;
            mailMessage.Headers.Add("Importance", "High");
            return client.SendMailAsync(mailMessage);
        }
    }
}
