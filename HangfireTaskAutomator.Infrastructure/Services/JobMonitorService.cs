using HangfireTaskAutomator.Core.Models;
using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HangfireTaskAutomator.Infrastructure.Services;

public class JobMonitorService : IJobMonitorService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobMonitorService> _logger;

    public JobMonitorService(ApplicationDbContext dbContext, ILogger<JobMonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task TrackJobStartAsync(string jobId, string taskName, string jobType)
    {
        _logger.LogInformation($"İş başlatıldı: {jobId} - {taskName} ({jobType})");

        var taskHistory = new TaskHistory
        {
            JobId = jobId,
            TaskName = taskName,
            JobType = jobType,
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };

        _dbContext.TaskHistorys.Add(taskHistory);
        await _dbContext.SaveChangesAsync();
    }

    public async Task TrackJobCompletionAsync(string jobId, string result)
    {
        _logger.LogInformation($"İş tamamlandı: {jobId}");

        var taskHistory = await _dbContext.TaskHistorys
            .FirstOrDefaultAsync(t => t.JobId == jobId && t.Status == "Running");

        if (taskHistory != null)
        {
            taskHistory.CompletedAt = DateTime.UtcNow;
            taskHistory.Status = "Completed";
            taskHistory.Result = result;

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task TrackJobFailureAsync(string jobId, Exception ex)
    {
        _logger.LogError(ex, $"İş başarısız oldu: {jobId}");

        var taskHistory = await _dbContext.TaskHistorys
            .FirstOrDefaultAsync(t => t.JobId == jobId && t.Status == "Running");

        if (taskHistory != null)
        {
            taskHistory.CompletedAt = DateTime.UtcNow;
            taskHistory.Status = "Failed";
            taskHistory.Exception = $"{ex.Message}\n{ex.StackTrace}";

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<TaskHistory>> GetRecentJobHistoryAsync(int count)
    {
        return await _dbContext.TaskHistorys
            .OrderByDescending(t => t.StartedAt)
            .Take(count)
            .ToListAsync();
    }
}
