[![](https://img.shields.io/nuget/v/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Hangfire.Util
### A general-purpose, reusable utility class for managing Hangfire background jobs

## Installation

```
dotnet add package Soenneker.Hangfire.Util
```

```csharp
builder.Services.Configure<HangfireUtilOptions>(options =>
{
    options.BatchSize = 200;
    options.NotifyOnUnhandledFailedJobs = true;

    options.ShouldDeleteFailedJob = job =>
    {
        // Example: Only delete jobs older than 7 days
        return job.FailedAt < DateTime.UtcNow.AddDays(-7);
    };

    options.ShouldDeleteSucceededJob = job =>
    {
        // Example: Delete all successful jobs older than 1 day
        return job.SucceededAt < DateTime.UtcNow.AddDays(-1);
    };
});

// Register the HangfireUtil service
builder.Services.AddSingleton<IHangfireUtil, HangfireUtil>();

RecurringJob.AddOrUpdate<IHangfireUtil>($"{nameof(IHangfireUtil.DeleteFailedJobs)}", c => c.DeleteFailedJobs(),  "0 0 * * *");
```