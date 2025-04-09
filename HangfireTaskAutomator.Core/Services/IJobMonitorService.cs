namespace HangfireTaskAutomator.Core.Services;

public interface IJobMonitorService
{
    Task TrackJobStartAsync(string jobId, string taskName, string jobType);
    Task TrackJobCompletionAsync(string jobId, string result);
    Task TrackJobFailureAsync(string jobId, Exception ex);
    Task<List<Core.Models.TaskHistory>> GetRecentJobHistoryAsync(int count);
}
