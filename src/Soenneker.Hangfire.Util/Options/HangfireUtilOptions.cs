using System;
using Hangfire.Storage.Monitoring;

namespace Soenneker.Hangfire.Util.Options;

public class HangfireUtilOptions
{
    public int BatchSize { get; set; } = 250;

    /// <summary>
    /// Determines whether a failed job should be deleted
    /// </summary>
    public Func<FailedJobDto, bool>? ShouldDeleteFailedJob { get; set; }

    /// <summary>
    /// Determines whether a succeeded job should be deleted
    /// </summary>
    public Func<SucceededJobDto, bool>? ShouldDeleteSucceededJob { get; set; }

    public bool NotifyOnUnhandledFailedJobs { get; set; } = true;
}