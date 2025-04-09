using HangfireTaskAutomator.Core.Models;
using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HangfireTaskAutomator.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ReportService> _logger;
    private readonly IEmailService _emailService;

    public ReportService(
        ApplicationDbContext dbContext,
        ILogger<ReportService> logger,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _emailService = emailService;
    }
    public async Task GenerateDailyReportAsync()
    {
        _logger.LogInformation("Günlük rapor oluşturuluyor...");

        // Rapor oluşturma simülasyonu
        await Task.Delay(3000);

        var reportPath = $"/reports/daily/daily-report-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

        var report = new ReportData
        {
            ReportName = $"Günlük Rapor {DateTime.UtcNow:yyyy-MM-dd}",
            ReportType = "Daily",
            GeneratedAt = DateTime.UtcNow,
            FilePath = reportPath,
            IsProcessed = false
        };

        _dbContext.ReportDatas.Add(report);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Günlük rapor oluşturuldu: {reportPath}");

        // Rapor oluşturuldu bilgisini e-posta ile gönder
        await _emailService.SendEmailAsync(
            "admin@example.com",
            $"Günlük Rapor Hazır: {DateTime.UtcNow:yyyy-MM-dd}",
            $"Günlük rapor başarıyla oluşturuldu. Raporu şu adresten indirebilirsiniz: {reportPath}"
        );
    }

    public async Task GenerateMonthlyReportAsync()
    {
        _logger.LogInformation("Aylık rapor oluşturuluyor...");

        // Rapor oluşturma simülasyonu
        await Task.Delay(8000);

        var reportPath = $"/reports/monthly/monthly-report-{DateTime.UtcNow:yyyy-MM}.pdf";

        var report = new ReportData
        {
            ReportName = $"Aylık Rapor {DateTime.UtcNow:yyyy-MM}",
            ReportType = "Monthly",
            GeneratedAt = DateTime.UtcNow,
            FilePath = reportPath,
            IsProcessed = false
        };

        _dbContext.ReportDatas.Add(report);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Aylık rapor oluşturuldu: {reportPath}");

        // Rapor oluşturuldu bilgisini e-posta ile gönder
        await _emailService.SendEmailAsync(
            "admin@example.com",
            $"Aylık Rapor Hazır: {DateTime.UtcNow:yyyy-MM}",
            $"Aylık rapor başarıyla oluşturuldu. Raporu şu adresten indirebilirsiniz: {reportPath}"
        );
    }

    public async Task GenerateWeeklyReportAsync()
    {
        _logger.LogInformation("Haftalık rapor oluşturuluyor...");

        // Rapor oluşturma simülasyonu
        await Task.Delay(5000);

        var weekNumber = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.UtcNow);
        var reportPath = $"/reports/weekly/weekly-report-{DateTime.UtcNow.Year}-W{weekNumber}.pdf";

        var report = new ReportData
        {
            ReportName = $"Haftalık Rapor {DateTime.UtcNow.Year}-W{weekNumber}",
            ReportType = "Weekly",
            GeneratedAt = DateTime.UtcNow,
            FilePath = reportPath,
            IsProcessed = false
        };

        _dbContext.ReportDatas.Add(report);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Haftalık rapor oluşturuldu: {reportPath}");

        // Rapor oluşturuldu bilgisini e-posta ile gönder
        await _emailService.SendEmailAsync(
            "admin@example.com",
            $"Haftalık Rapor Hazır: {DateTime.UtcNow.Year}-W{weekNumber}",
            $"Haftalık rapor başarıyla oluşturuldu. Raporu şu adresten indirebilirsiniz: {reportPath}"
        );
    }

    public async Task ProcessPendingReportsAsync()
    {
        _logger.LogInformation("Bekleyen raporlar işleniyor...");

        var pendingReports = await _dbContext.ReportDatas
            .Where(r => !r.IsProcessed)
            .ToListAsync();

        _logger.LogInformation($"İşlenecek rapor sayısı: {pendingReports.Count}");

        foreach (var report in pendingReports)
        {
            try
            {
                // Rapor işleme simülasyonu
                await Task.Delay(1000);

                report.IsProcessed = true;

                _logger.LogInformation($"Rapor işlendi: {report.Id} - {report.ReportName}");

                // Rapor işlendi bilgisini e-posta ile gönder
                await _emailService.SendEmailAsync(
                    "reports@example.com",
                    $"Rapor İşlendi: {report.ReportName}",
                    $"Rapor başarıyla işlendi: {report.ReportName}. Dosya yolu: {report.FilePath}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rapor işlenirken hata: {report.Id} - {report.ReportName}");
            }
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Bekleyen raporların işlenmesi tamamlandı");
    }
}
