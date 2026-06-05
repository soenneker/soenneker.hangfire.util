using System;
using Hangfire.Storage.Monitoring;

namespace Soenneker.Hangfire.Util.Options;

/// <summary>
/// Represents the hangfire util options.
/// </summary>
public class HangfireUtilOptions
{
    /// <summary>
    /// Gets or sets batch size.
    /// </summary>
    public int BatchSize { get; set; } = 250;

    /// <summary>
    /// Determines whether a failed job should be deleted
    /// </summary>
    public Func<FailedJobDto, bool>? ShouldDeleteFailedJob { get; set; }

    /// <summary>
    /// Determines whether a succeeded job should be deleted
    /// </summary>
    public Func<SucceededJobDto, bool>? ShouldDeleteSucceededJob { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether notify on unhandled failed jobs.
    /// </summary>
    public bool NotifyOnUnhandledFailedJobs { get; set; } = true;
}