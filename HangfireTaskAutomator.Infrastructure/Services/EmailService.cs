using HangfireTaskAutomator.Core.Models;
using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HangfireTaskAutomator.Infrastructure.Services;

public class EmailService(ApplicationDbContext _dbContext, ILogger<EmailService> _logger) : IEmailService
{
    public async Task<int> QueueEmail(string recipient, string subject, string body)
    {
        {
            var email = new Email
            {
                Recipient = recipient,
                Subject = subject,
                Body = body,
                IsSent = false
            };

            _dbContext.Emails.Add(email);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"E-posta kuyruğa eklendi: {email.Id} - {recipient}");

            return email.Id;
        }
    }

    public async Task SendBulkEmailsAsync(List<string> recipients, string subject, string body)
    {
        _logger.LogInformation($"Toplu e-posta gönderiliyor - Alıcı sayısı: {recipients.Count}");

        var emails = recipients.Select(recipient => new Email
        {
            Recipient = recipient,
            Subject = subject,
            Body = body,
            IsSent = false
        }).ToList();

        await _dbContext.Emails.AddRangeAsync(emails);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Toplu e-postalar kuyruğa eklendi");
    }

    public async Task SendEmailAsync(string recipient, string subject, string body)
    {
        _logger.LogInformation($"E-posta gönderiliyor: {recipient}, Konu: {subject}");

        // E-posta gönderme simülasyonu (gerçek projede SMTP entegrasyonu yapılır)
        await Task.Delay(1000); // Ağ gecikmesini simüle ediyoruz

        // Gönderilen e-postayı kaydet
        var email = new Email
        {
            Recipient = recipient,
            Subject = subject,
            Body = body,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _dbContext.Emails.Add(email);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"E-posta başarıyla gönderildi: {recipient}");
    }

    public async Task SendPendingEmailsAsync()
    {
        _logger.LogInformation("Bekleyen e-postalar gönderiliyor");

        var pendingEmails = await _dbContext.Emails
            .Where(e => !e.IsSent)
            .ToListAsync();

        _logger.LogInformation($"Bekleyen e-posta sayısı: {pendingEmails.Count}");

        foreach (var email in pendingEmails)
        {
            try
            {
                // E-posta gönderme simülasyonu
                await Task.Delay(500);

                email.IsSent = true;
                email.SentAt = DateTime.UtcNow;

                _logger.LogInformation($"Bekleyen e-posta gönderildi: {email.Id} - {email.Recipient}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta gönderilirken hata: {email.Id} - {email.Recipient}");
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Bekleyen e-postaların gönderilmesi tamamlandı");
    }
}
