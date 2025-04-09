namespace HangfireTaskAutomator.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string recipient, string subject, string body); // This method sends an email immediately.
    Task SendPendingEmailsAsync(); // This method processes and sends any pending emails in the queue.
    Task SendBulkEmailsAsync(List<string> recipients, string subject, string body); // This method sends bulk emails to a list of recipients.
    Task<int> QueueEmail(string recipient, string subject, string body); // This method queues an email for later processing.
}
