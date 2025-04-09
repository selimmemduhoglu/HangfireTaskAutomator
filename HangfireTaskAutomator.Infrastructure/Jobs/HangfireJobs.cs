using Hangfire;
using HangfireTaskAutomator.Core.Services;
using Microsoft.Extensions.Logging;

namespace HangfireTaskAutomator.Infrastructure.Jobs;

public class HangfireJobs
{
    private readonly IEmailService _emailService;
    private readonly IReportService _reportService;
    private readonly IDataCleanupService _dataCleanupService;
    private readonly IJobMonitorService _jobMonitorService;
    private readonly ILogger<HangfireJobs> _logger;

    public HangfireJobs(
        IEmailService emailService,
        IReportService reportService,
        IDataCleanupService dataCleanupService,
        IJobMonitorService jobMonitorService,
        ILogger<HangfireJobs> logger)
    {
        _emailService = emailService;
        _reportService = reportService;
        _dataCleanupService = dataCleanupService;
        _jobMonitorService = jobMonitorService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 600 })]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task SendPendingEmails()
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Bekleyen E-postaları Gönder", "Recurring");
            await _emailService.SendPendingEmailsAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Bekleyen e-postalar başarıyla gönderildi");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw; // Hangfire'ın yeniden denemesi için hata fırlat
        }
    }

    [AutomaticRetry(Attempts = 2)]
    [Queue("reports")]
    public async Task GenerateDailyReport()
    {
        var jobId = Guid.NewGuid().ToString();

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Günlük Rapor Oluştur", "Daily");
            await _reportService.GenerateDailyReportAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Günlük rapor başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 2)]
    [Queue("reports")]
    public async Task GenerateWeeklyReport()
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Haftalık Rapor Oluştur", "Weekly");
            await _reportService.GenerateWeeklyReportAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Haftalık rapor başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 2)]
    [Queue("reports")]
    public async Task GenerateMonthlyReport()
    {
        var jobId = Guid.NewGuid().ToString();

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Aylık Rapor Oluştur", "Monthly");
            await _reportService.GenerateMonthlyReportAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Aylık rapor başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessPendingReports()
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Bekleyen Raporları İşle", "Recurring");
            await _reportService.ProcessPendingReportsAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Bekleyen raporlar başarıyla işlendi");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 1)]
    [Queue("maintenance")]
    public async Task CleanupOldData(int daysToKeep = 90)
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Eski Verileri Temizle", "Maintenance");
            await _dataCleanupService.CleanupOldDataAsync(daysToKeep);
            await _jobMonitorService.TrackJobCompletionAsync(jobId, $"Eski veriler başarıyla temizlendi ({daysToKeep} günden eski veriler)");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 1)]
    [Queue("maintenance")]
    public async Task ArchiveData()
    {
        var jobId = Guid.NewGuid().ToString();

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Verileri Arşivle", "Maintenance");
            await _dataCleanupService.ArchiveDataAsync();
            await _jobMonitorService.TrackJobCompletionAsync(jobId, "Veriler başarıyla arşivlendi");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [Queue("emails")]
    public async Task SendNotificationEmail(string recipient, string subject, string body)
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Bildirim E-postası Gönder", "Fire-and-forget");
            await _emailService.SendEmailAsync(recipient, subject, body);
            await _jobMonitorService.TrackJobCompletionAsync(jobId, $"Bildirim e-postası başarıyla gönderildi: {recipient}");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [Queue("emails")]
    public async Task<int> QueueNotificationEmail(string recipient, string subject, string body)
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Bildirim E-postası Kuyruğa Ekle", "Fire-and-forget");
            var emailId = await _emailService.QueueEmail(recipient, subject, body);
            await _jobMonitorService.TrackJobCompletionAsync(jobId, $"Bildirim e-postası kuyruğa eklendi: {emailId}");
            return emailId;
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }

    [Queue("emails")]
    public void SendDelayedEmail(string recipient, string subject, string body, TimeSpan delay)
    {
        _logger.LogInformation($"Gecikmeli e-posta planlandı: {recipient}, Gecikme: {delay.TotalMinutes} dakika");

        var jobId = BackgroundJob.Schedule(
            () => SendNotificationEmail(recipient, subject, body),
            delay);

        _logger.LogInformation($"Gecikmeli e-posta için iş kimliği: {jobId}");
    }

    [Queue("default")]
    public async Task ContinuationJobExample(int previousJobResult)
    {
        var jobId = Guid.NewGuid().ToString(); 

        try
        {
            await _jobMonitorService.TrackJobStartAsync(jobId, "Devam İşi Örneği", "Continuation");
            _logger.LogInformation($"Devam işi çalışıyor. Önceki işin sonucu: {previousJobResult}");
            await Task.Delay(1000);
            await _jobMonitorService.TrackJobCompletionAsync(jobId, $"Devam işi tamamlandı. Girdi: {previousJobResult}");
        }
        catch (Exception ex)
        {
            await _jobMonitorService.TrackJobFailureAsync(jobId, ex);
            throw;
        }
    }
}
