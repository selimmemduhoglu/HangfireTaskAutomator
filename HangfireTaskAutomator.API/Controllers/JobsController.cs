using Hangfire;
using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace HangfireTaskAutomator.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IJobMonitorService _jobMonitorService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly HangfireJobs _hangfireJobs;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobMonitorService jobMonitorService,
        IBackgroundJobClient backgroundJobClient,
        HangfireJobs hangfireJobs,
        ILogger<JobsController> logger)
    {
        _jobMonitorService = jobMonitorService;
        _backgroundJobClient = backgroundJobClient;
        _hangfireJobs = hangfireJobs;
        _logger = logger;
    }



    [HttpGet("history")]
    public async Task<IActionResult> GetRecentJobHistory([FromQuery] int count = 20)
    {
        var history = await _jobMonitorService.GetRecentJobHistoryAsync(count);
        return Ok(history);
    }

    [HttpPost("send-email")]
    public IActionResult QueueEmail([FromBody] EmailRequest request)
    {
        _logger.LogInformation($"E-posta kuyruğa ekleme isteği: {request.Recipient}");

        var jobId = _backgroundJobClient.Enqueue<HangfireJobs>(
            job => job.SendNotificationEmail(request.Recipient, request.Subject, request.Body));

        _logger.LogInformation($"E-posta işi kuyruğa eklendi: {jobId}");

        return Ok(new { JobId = jobId });
    }



    public IActionResult QueueDelayedEmail([FromBody] DelayedEmailRequest request)
    {
        _logger.LogInformation($"Gecikmeli e-posta kuyruğa ekleme isteği: {request.Recipient}, Gecikme: {request.DelayMinutes} dakika");

        var delay = TimeSpan.FromMinutes(request.DelayMinutes);

        var jobId = _backgroundJobClient.Schedule<HangfireJobs>(
            job => job.SendNotificationEmail(request.Recipient, request.Subject, request.Body),
            delay);

        _logger.LogInformation($"Gecikmeli e-posta işi kuyruğa eklendi: {jobId}");

        return Ok(new { JobId = jobId });
    }

    [HttpPost("bulk-email")]
    public IActionResult QueueBulkEmail([FromBody] BulkEmailRequest request)
    {
        _logger.LogInformation($"Toplu e-posta kuyruğa ekleme isteği: Alıcı sayısı: {request.Recipients.Count}");

        var jobId = _backgroundJobClient.Enqueue<IEmailService>(
            service => service.SendBulkEmailsAsync(request.Recipients, request.Subject, request.Body));

        _logger.LogInformation($"Toplu e-posta işi kuyruğa eklendi: {jobId}");

        return Ok(new { JobId = jobId });
    }


    public IActionResult GenerateReport([FromBody] ReportRequest request)
    {
        _logger.LogInformation($"Rapor oluşturma isteği: {request.ReportType}");

        string jobId;

        switch (request.ReportType.ToLower())
        {
            case "daily":
                jobId = _backgroundJobClient.Enqueue<HangfireJobs>(
                    job => job.GenerateDailyReport());
                break;

            case "weekly":
                jobId = _backgroundJobClient.Enqueue<HangfireJobs>(
                    job => job.GenerateWeeklyReport());
                break;

            case "monthly":
                jobId = _backgroundJobClient.Enqueue<HangfireJobs>(
                    job => job.GenerateMonthlyReport());
                break;

            default:
                return BadRequest(new { Error = "Geçersiz rapor türü. Desteklenen değerler: daily, weekly, monthly" });
        }

        _logger.LogInformation($"Rapor oluşturma işi kuyruğa eklendi: {jobId}");

        return Ok(new { JobId = jobId });
    }



    public IActionResult CleanupData([FromBody] CleanupRequest request)
    {
        _logger.LogInformation($"Veri temizleme isteği: {request.DaysToKeep} günden eski veriler");

        var jobId = _backgroundJobClient.Enqueue<HangfireJobs>(
            job => job.CleanupOldData(request.DaysToKeep));

        _logger.LogInformation($"Veri temizleme işi kuyruğa eklendi: {jobId}");

        return Ok(new { JobId = jobId });
    }



    [HttpPost("job-chain")]
    public IActionResult CreateJobChain([FromBody] EmailRequest request)
    {
        _logger.LogInformation($"İş zinciri oluşturma isteği başlatıldı");

        // İlk iş: E-posta kuyruğa ekle
        var jobId1 = _backgroundJobClient.Enqueue<HangfireJobs>(
            job => job.QueueNotificationEmail(request.Recipient, request.Subject, request.Body));

        // İkinci iş: İlk iş tamamlandığında bekleyen e-postaları gönder
        var jobId2 = _backgroundJobClient.ContinueJobWith<HangfireJobs>(
            jobId1,
            job => job.SendPendingEmails());

        // Üçüncü iş: İkinci iş tamamlandığında devam işi çalıştır
        var jobId3 = _backgroundJobClient.ContinueJobWith<HangfireJobs, int>(
            jobId2,
            job => job.ContinuationJobExample(123));

        _logger.LogInformation($"İş zinciri oluşturuldu: {jobId1} -> {jobId2} -> {jobId3}");

        return Ok(new
        {
            JobChain = new[] { jobId1, jobId2, jobId3 },
            Message = "İş zinciri başarıyla oluşturuldu"
        });
    }



    public class EmailRequest
    {
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class DelayedEmailRequest : EmailRequest
    {
        public int DelayMinutes { get; set; } = 5;
    }

    public class BulkEmailRequest
    {
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class ReportRequest
    {
        public string ReportType { get; set; }
    }

    public class CleanupRequest
    {
        public int DaysToKeep { get; set; } = 90;
    }



}
