namespace HangfireTaskAutomator.Core.Services;

public interface IReportService
{
    Task GenerateDailyReportAsync();
    Task GenerateWeeklyReportAsync();
    Task GenerateMonthlyReportAsync();
    Task ProcessPendingReportsAsync();
}
