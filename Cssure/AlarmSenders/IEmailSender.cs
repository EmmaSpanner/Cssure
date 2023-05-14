namespace Cssure.AlarmSenders
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string[] emails, string subject, string message);
    }
}
