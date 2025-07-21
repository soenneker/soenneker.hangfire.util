using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Soenneker.Hangfire.Util.Abstract;
using Soenneker.Hangfire.Util.Options;
using System;

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

    private int PageDelete<TDto>(Func<int, int, JobList<TDto>> fetchPage, Func<TDto?, bool> shouldDelete, Action<TDto?, string>? whenSkip = null)
        where TDto : class
    {
        IStorageConnection? conn = JobStorage.Current.GetConnection();
        int batch = _options.BatchSize;
        var deleted = 0;

        while (true)
        {
            JobList<TDto> page = fetchPage(0, batch);

            if (page.Count == 0)
                break;

            using IWriteOnlyTransaction? tx = conn.CreateWriteTransaction();

            foreach ((string jobId, TDto? dto) in page)
            {
                if (!shouldDelete(dto))
                {
                    whenSkip?.Invoke(dto, jobId);
                    continue;
                }

                tx.ExpireJob(jobId, TimeSpan.Zero); // marks every related key expired
                deleted++;
            }

            tx.Commit(); // one round‑trip per page

            if (page.Count < batch)
                break; // nothing left
        }

        return deleted;
    }

    public void DeleteFailedJobs()
    {
        _logger.LogInformation("Starting deletion of failed jobs...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            int deleted = PageDelete(monitor.FailedJobs, dto => dto?.Job == null || (_options.ShouldDeleteFailedJob?.Invoke(dto) ?? false), (dto, key) =>
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
            int deleted = PageDelete(monitor.FailedJobs, _ => true); // delete every key returned


            _logger.LogInformation("Deleted {Count} failed jobs.", deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting failed jobs");
            throw;
        }
    }

    public void DeleteSuccessfulJobs()
    {
        _logger.LogInformation("Deleting successful jobs...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            int deleted = PageDelete(monitor.SucceededJobs, dto => _options.ShouldDeleteSucceededJob?.Invoke(dto) ?? false);

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
}