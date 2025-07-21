using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Soenneker.Hangfire.Util.Abstract;
using Soenneker.Hangfire.Util.Options;
using System;
using Hangfire.States;

namespace Soenneker.Hangfire.Util;

/// <inheritdoc cref="IHangfireUtil"/>
public sealed class HangfireUtil : IHangfireUtil
{
    private readonly ILogger<HangfireUtil> _logger;
    private readonly HangfireUtilOptions _options;

    public HangfireUtil(ILogger<HangfireUtil> logger, IOptions<HangfireUtilOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    private int PageDelete<TDto>(
        string setName,
        Func<int, int, JobList<TDto>> fetchPage,
        Func<TDto?, bool> shouldDelete,
        Action<TDto?, string>? whenSkip = null)
        where TDto : class
    {
        IStorageConnection? conn = JobStorage.Current.GetConnection();
        int batch = _options.BatchSize;
        var deleted = 0;
        var offset = 0;        

        while (true)
        {
            JobList<TDto> page = fetchPage(offset, batch);
            if (page.Count == 0) break;

            using IWriteOnlyTransaction tx = conn.CreateWriteTransaction();

            foreach ((string jobId, TDto? dto) in page)
            {
                if (!shouldDelete(dto))
                {
                    whenSkip?.Invoke(dto, jobId);
                    offset++;               // skip past it so we don’t see it again
                    continue;
                }

                tx.SetJobState(jobId, new DeletedState());
                tx.RemoveFromSet(setName, jobId);
                tx.ExpireJob(jobId, TimeSpan.Zero);
                deleted++;
            }

            tx.Commit();

            // If we deleted *anything* the set got smaller, so keep the same offset.
            // If we skipped everything, we advanced offset above, so we still progress.
            if (page.Count < batch) break;  // reached end even under heavy load
        }

        return deleted;
    }


    public void DeleteFailedJobs()
    {
        _logger.LogInformation("Starting deletion of failed jobs...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            int deleted = PageDelete("failed", monitor.FailedJobs, dto => dto?.Job == null || (_options.ShouldDeleteFailedJob?.Invoke(dto) ?? false),
                (dto, key) =>
                {
                    if (_options.NotifyOnUnhandledFailedJobs && dto != null)
                        _logger.LogWarning("Unhandled failed job {Key} ({Type})\nDetails: {Details}", key, dto.ExceptionType, dto.ExceptionDetails);
                });

            _logger.LogInformation("Deleted {Count} failed jobs.", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting failed jobs");
            throw;
        }
    }

    public void DeleteFailedJobsSilently()
    {
        _logger.LogInformation("Deleting failed jobs (silent)...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            int deleted = PageDelete("failed", monitor.FailedJobs, _ => true); // delete every key returned


            _logger.LogInformation("Deleted {Count} failed jobs.", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting failed jobs");
            throw;
        }
    }

    public void DeleteSucceededJobs()
    {
        _logger.LogInformation("Deleting succeeded jobs...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            int deleted = PageDelete("succeeded", monitor.SucceededJobs, dto => _options.ShouldDeleteSucceededJob?.Invoke(dto) ?? false);

            _logger.LogInformation("Deleted {Count} successful jobs.", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting successful jobs");
            throw;
        }
    }

    public void DeleteExistingRecurringJobs()
    {
        using IStorageConnection? connection = JobStorage.Current.GetConnection();
        var removed = 0;

        foreach (RecurringJobDto job in connection.GetRecurringJobs())
        {
            RecurringJob.RemoveIfExists(job.Id);
            removed++;
        }

        _logger.LogInformation("Removed {Count} recurring jobs.", removed);
    }

    public void PurgeHangfireGarbage()
    {
        IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();

        // 1) failed jobs whose *reason* is “Job expired”
        int expired = PageDelete("failed", monitor.FailedJobs, IsJobExpired);

        // 2) everything that still sits in the deleted bucket
        int alreadyDeleted = PageDelete("deleted", monitor.DeletedJobs, _ => true);

        _logger.LogInformation("Purged {Expired} expired-failed + {Deleted} deleted jobs.", expired, alreadyDeleted);
    }

    private static bool IsJobExpired(FailedJobDto? dto)
    {
        // null hash? -> already expired
        if (dto == null) return true;

        // Hangfire puts "Job expired" into Reason; sometimes ExceptionMessage too.
        return (dto.Reason?.Contains("Job expired", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (dto.ExceptionMessage?.Contains("expired", StringComparison.OrdinalIgnoreCase) ?? false) ||
               (dto.ExceptionType?.Contains("Expired", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}