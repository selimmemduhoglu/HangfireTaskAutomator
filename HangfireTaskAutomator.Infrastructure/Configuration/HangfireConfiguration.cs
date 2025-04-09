using Hangfire;
using Hangfire.SqlServer;
using HangfireTaskAutomator.Core.Services;
using HangfireTaskAutomator.Infrastructure.Jobs;
using HangfireTaskAutomator.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaskAutomator.Infrastructure.Configuration
{
    public static class HangfireConfiguration
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Veritabanı bağlantı dizesini al
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Hangfire hizmetlerini ekle
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                })
                .WithJobExpirationTimeout(TimeSpan.FromDays(7))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 })
            );

            // Kuyruklar için işçi sayılarını yapılandır
            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "emails", "reports", "maintenance" };
                options.WorkerCount = 10;
                options.ServerName = $"TaskAutomator:{Environment.MachineName}";
            });

            // İş sınıfını kaydet
            services.AddScoped<HangfireJobs>();

            // Servis kayıtları
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IDataCleanupService, DataCleanupService>();
            services.AddScoped<IJobMonitorService, JobMonitorService>();

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                DashboardTitle = "TaskAutomator - İş Zamanlayıcı",
                DisplayStorageConnectionString = false,
                IsReadOnlyFunc = context => false
            });

            return app;
        }

        public static void ScheduleRecurringJobs()
        {
            // Tekrarlayan işleri planla
            var recurringJobManager = new RecurringJobManager();

            // E-posta işleri
            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "send-pending-emails",
                job => job.SendPendingEmails(),
                Cron.MinuteInterval(15)
            );

            // Rapor işleri
            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "daily-report",
                job => job.GenerateDailyReport(),
                Cron.Daily(7, 0)
            );

            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "weekly-report",
                job => job.GenerateWeeklyReport(),
                Cron.Weekly(DayOfWeek.Monday, 6, 0)
            );

            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "monthly-report",
                job => job.GenerateMonthlyReport(),
                Cron.Monthly(1, 5, 0)
            );

            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "process-pending-reports",
                job => job.ProcessPendingReports(),
                Cron.HourInterval(2)
            );

            // Bakım işleri
            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "cleanup-old-data",
                job => job.CleanupOldData(90),
                Cron.Weekly(DayOfWeek.Sunday, 2, 0)
            );

            recurringJobManager.AddOrUpdate<HangfireJobs>(
                "archive-data",
                job => job.ArchiveData(),
                Cron.Weekly(DayOfWeek.Sunday, 3, 0)
            );
        }
    }

    // Hangfire Dashboard için basit bir kimlik doğrulama filtreleme sınıfı
    public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
    {
        public bool Authorize(Hangfire.Dashboard.DashboardContext context)
        {
            // Gerçek projede uygun kimlik doğrulama ekleyebilirsiniz
            // Örneğin:
            // var httpContext = context.GetHttpContext();
            // return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("Admin");

            // Geliştirme ortamında herzaman true döndür
            return true;
        }
    }
}

    public static class HangfireConfiguration
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Veritabanı bağlantı dizesini al
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Hangfire hizmetlerini ekle
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            })
            .WithJobExpirationTimeout(TimeSpan.FromDays(7))
            .UseFilter(new AutomaticRetryAttribute { Attempts = 3 })
        );

        // Kuyruklar için işçi sayılarını yapılandır
        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "default", "emails", "reports", "maintenance" };
            options.WorkerCount = 10;
            options.ServerName = $"TaskAutomator:{Environment.MachineName}";
        });

        // İş sınıfını kaydet
        services.AddScoped<HangfireJobs>();

        // Servis kayıtları
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDataCleanupService, DataCleanupService>();
        services.AddScoped<IJobMonitorService, JobMonitorService>();

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            DashboardTitle = "TaskAutomator - İş Zamanlayıcı",
            DisplayStorageConnectionString = false,
            IsReadOnlyFunc = context => false
        });

        return app;
    }

    public static void ScheduleRecurringJobs()
    {
        // Tekrarlayan işleri planla
        var recurringJobManager = new RecurringJobManager();

        // E-posta işleri
        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "send-pending-emails",
            job => job.SendPendingEmails(),
            Cron.MinuteInterval(15)
        );

        // Rapor işleri
        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "daily-report",
            job => job.GenerateDailyReport(),
            Cron.Daily(7, 0)
        );

        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "weekly-report",
            job => job.GenerateWeeklyReport(),
            Cron.Weekly(DayOfWeek.Monday, 6, 0)
        );

        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "monthly-report",
            job => job.GenerateMonthlyReport(),
            Cron.Monthly(1, 5, 0)
        );

        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "process-pending-reports",
            job => job.ProcessPendingReports(),
            Cron.HourInterval(2)
        );

        // Bakım işleri
        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "cleanup-old-data",
            job => job.CleanupOldData(90),
            Cron.Weekly(DayOfWeek.Sunday, 2, 0)
        );

        recurringJobManager.AddOrUpdate<HangfireJobs>(
            "archive-data",
            job => job.ArchiveData(),
            Cron.Weekly(DayOfWeek.Sunday, 3, 0)
        );
    }
}

// Hangfire Dashboard için basit bir kimlik doğrulama filtreleme sınıfı
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Gerçek projede uygun kimlik doğrulama ekleyebilirsiniz
        // Örneğin:
        // var httpContext = context.GetHttpContext();
        // return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("Admin");

        // Geliştirme ortamında herzaman true döndür
        return true;
    }
}

