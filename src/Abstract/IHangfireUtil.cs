using Soenneker.Hangfire.SkipMissedRuns;

namespace Soenneker.Hangfire.Util.Abstract;

/// <summary>
/// A general-purpose, reusable utility class for managing Hangfire background jobs
/// </summary>
public interface IHangfireUtil
{
    /// <summary>
    /// Deletes failed Hangfire jobs based on filtering options. Logs unhandled jobs if enabled.
    /// </summary>
    [SkipMissedRuns]
    void DeleteFailedJobs();

    /// <summary>
    /// Deletes all failed Hangfire jobs without logging unhandled ones.
    /// </summary>
    [SkipMissedRuns]
    void DeleteFailedJobsSilently();

    /// <summary>
    /// Deletes succeeded Hangfire jobs based on filtering options.
    /// </summary>
    [SkipMissedRuns]
    void DeleteSuccessfulJobs();

    /// <summary>
    /// Removes all currently scheduled recurring Hangfire jobs.
    /// </summary>
    [SkipMissedRuns]
    void DeleteExistingRecurringJobs();
}