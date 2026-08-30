using System;
using Hangfire.Storage.Monitoring;

namespace Soenneker.Hangfire.Util.Options;

/// <summary>
/// Configures Hangfire job cleanup policies.
/// </summary>
public class HangfireUtilOptions
{
    /// <summary>
    /// Gets or sets the number of jobs read per storage page. Values below one are treated as one.
    /// </summary>
    public int BatchSize { get; set; } = 250;

    /// <summary>
    /// Gets or sets the predicate that selects failed jobs for deletion.
    /// </summary>
    public Func<FailedJobDto, bool>? ShouldDeleteFailedJob { get; set; }

    /// <summary>
    /// Gets or sets the predicate that selects succeeded jobs for deletion.
    /// </summary>
    public Func<SucceededJobDto, bool>? ShouldDeleteSucceededJob { get; set; }

    /// <summary>
    /// Gets or sets whether retained failed jobs are written to the log.
    /// </summary>
    public bool NotifyOnUnhandledFailedJobs { get; set; } = true;
}
