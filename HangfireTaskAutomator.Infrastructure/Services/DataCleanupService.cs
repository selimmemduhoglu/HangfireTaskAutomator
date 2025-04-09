using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HangfireTaskAutomator.Infrastructure.Services;

public class DataCleanupService :IDataCleanupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DataCleanupService> _logger;

    public DataCleanupService(ApplicationDbContext dbContext, ILogger<DataCleanupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ArchiveDataAsync()
    {
        _logger.LogInformation("Veriler arşivleniyor...");

        // Arşivleme işlemi simülasyonu
        await Task.Delay(5000);

        _logger.LogInformation($"Arşivleme tamamlandı: {DateTime.UtcNow}");
    }

    public async Task CleanupOldDataAsync(int daysToKeep)
    {
        _logger.LogInformation($"Eski veriler temizleniyor. Korunacak gün sayısı: {daysToKeep}");

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        // Eski e-posta kayıtlarını temizle
        var oldEmails = await _dbContext.Emails
            .Where(e => e.CreatedAt < cutoffDate && e.IsSent)
            .ToListAsync();

        _logger.LogInformation($"Silinecek e-posta sayısı: {oldEmails.Count}");
        _dbContext.Emails.RemoveRange(oldEmails);

        // Eski rapor kayıtlarını temizle
        var oldReports = await _dbContext.ReportDatas
            .Where(r => r.CreatedAt < cutoffDate && r.IsProcessed)
            .ToListAsync();

        _logger.LogInformation($"Silinecek rapor sayısı: {oldReports.Count}");
        _dbContext.ReportDatas.RemoveRange(oldReports);

        // Eski görev geçmişini temizle
        var oldTaskHistory = await _dbContext.TaskHistorys
            .Where(t => t.StartedAt < cutoffDate && t.CompletedAt.HasValue)
            .ToListAsync();

        _logger.LogInformation($"Silinecek görev geçmişi sayısı: {oldTaskHistory.Count}");
        _dbContext.TaskHistorys.RemoveRange(oldTaskHistory);

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Veri temizleme işlemi tamamlandı");
    }
}
