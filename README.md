[![](https://img.shields.io/nuget/v/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/build-and-test.yml?style=for-the-badge&label=build)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/nuget/dt/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/codeql.yml)

# Soenneker.Hangfire.Util

Provides policy-driven cleanup operations for the current Hangfire job storage: selective failed and succeeded job deletion, full failed-job deletion, recurring-job removal, and purging of expired/deleted entries.

## Installation

```bash
dotnet add package Soenneker.Hangfire.Util
```

## Configure cleanup policies

```csharp
using Soenneker.Hangfire.Util.Options;
using Soenneker.Hangfire.Util.Registrars;

services.Configure<HangfireUtilOptions>(options =>
{
    options.BatchSize = 250;
    options.NotifyOnUnhandledFailedJobs = true;

    options.ShouldDeleteFailedJob = job =>
        job.FailedAt < DateTime.UtcNow.AddDays(-7);

    options.ShouldDeleteSucceededJob = job =>
        job.SucceededAt < DateTime.UtcNow.AddDays(-1);
});

services.AddHangfireUtilAsScoped();
```

The utility operates through `JobStorage.Current`, so configure Hangfire storage before invoking it. A singleton registration is also available. The class does not own or dispose Hangfire's storage connection.

## Run selective cleanup

```csharp
using Soenneker.Hangfire.Util.Abstract;

hangfireUtil.DeleteFailedJobs();
hangfireUtil.DeleteSucceededJobs();
```

These methods delete only jobs accepted by the corresponding predicate. With no predicate, normal jobs are retained, but entries whose job payload has already disappeared are still removed. When notifications are enabled, retained failed jobs are logged as warnings.

## Destructive operations

```csharp
hangfireUtil.DeleteFailedJobsSilently();       // every failed job
hangfireUtil.DeleteExistingRecurringJobs();   // every recurring definition
hangfireUtil.PurgeHangfireGarbage();           // explicitly expired failures and deleted entries
```

`DeleteFailedJobsSilently()` is “silent” only with respect to retained-job warnings; it still logs the cleanup summary. `PurgeHangfireGarbage()` deletes failures whose Hangfire reason is exactly `Job expired`, plus everything already in the deleted set. It does not delete application failures merely because their exception mentions expiration.

Cleanup pages through storage in `BatchSize` chunks and commits each page separately. It is not atomic across the whole data set. Back up production storage and stop competing cleanup processes before broad deletion. `DeleteExistingRecurringJobs()` removes definitions, not merely queued occurrences, so they will not run again until registered again.

The cleanup methods carry `SkipMissedRunsAttribute`, making them suitable for recurring maintenance schedules without replaying stale cleanup occurrences after a scheduler outage.
