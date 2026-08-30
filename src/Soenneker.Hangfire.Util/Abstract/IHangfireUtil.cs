using Soenneker.Hangfire.SkipMissedRuns;

namespace Soenneker.Hangfire.Util.Abstract;

/// <summary>
/// Performs destructive, policy-driven cleanup of Hangfire job storage.
/// </summary>
public interface IHangfireUtil
{
    /// <summary>
    /// Deletes failed jobs selected by the configured predicate and optionally logs jobs the predicate retains.
    /// </summary>
    [SkipMissedRuns]
    void DeleteFailedJobs();

    /// <summary>
    /// Deletes every failed job without evaluating the configured failed-job predicate.
    /// </summary>
    [SkipMissedRuns]
    void DeleteFailedJobsSilently();

    /// <summary>
    /// Deletes succeeded jobs selected by the configured predicate.
    /// </summary>
    [SkipMissedRuns]
    void DeleteSucceededJobs();

    /// <summary>
    /// Removes every recurring job definition from the current Hangfire storage.
    /// </summary>
    [SkipMissedRuns]
    void DeleteExistingRecurringJobs();

    /// <summary>
    /// Purges failed entries explicitly marked with Hangfire's <c>Job expired</c> reason and all entries in the deleted set.
    /// </summary>
    [SkipMissedRuns]
    void PurgeHangfireGarbage();
}
