namespace HangfireTaskAutomator.Core.Services;

public interface IDataCleanupService
{
    Task CleanupOldDataAsync(int daysToKeep);
    Task ArchiveDataAsync();
}
