using System;
using Hangfire.Storage.Monitoring;

namespace Soenneker.Hangfire.Util.Options;

public class HangfireUtilOptions
{
    public Func<FailedJobDto, bool>? ShouldDeleteFailedJob { get; set; }

    public Func<SucceededJobDto, bool>? ShouldDeleteSucceededJob { get; set; }

    public int BatchSize { get; set; } = 250;

    public bool NotifyOnUnhandledFailedJobs { get; set; } = true;
}