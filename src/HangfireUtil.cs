using Hangfire;
using Soenneker.Hangfire.Util.Abstract;
using Soenneker.Hangfire.Util.Options;
using System;
using System.Collections.Generic;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Soenneker.Hangfire.Util;

///<inheritdoc cref="IHangfireUtil"/>
public class HangfireUtil : IHangfireUtil
{
    private readonly ILogger<HangfireUtil> _logger;
    private readonly HangfireUtilOptions _options;

    public HangfireUtil(ILogger<HangfireUtil> logger, IOptions<HangfireUtilOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public void DeleteFailedJobs()
    {
        _logger.LogInformation("Starting deletion of failed jobs...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            long count = monitor.FailedCount();

            if (count == 0)
            {
                _logger.LogDebug("No failed jobs to delete.");
                return;
            }

            int batch = _options.BatchSize;
            var pageCount = (int) ((count + batch - 1) / batch); // ceil(count / batch)
            var toDelete = new List<string>(Math.Min((int) count, 1000));

            for (var i = 0; i < pageCount; i++)
            {
                JobList<FailedJobDto>? jobs = monitor.FailedJobs(i * batch, batch);

                foreach ((string? key, FailedJobDto? job) in jobs)
                {
                    if (job?.Job == null || (_options.ShouldDeleteFailedJob?.Invoke(job) ?? false))
                    {
                        toDelete.Add(key);
                    }
                    else if (_options.NotifyOnUnhandledFailedJobs)
                    {
                        _logger.LogWarning("Unhandled failed job: {key} ({type})\nDetails: {details}", key, job.ExceptionType, job.ExceptionDetails);
                    }
                }
            }

            for (var i = 0; i < toDelete.Count; i++)
            {
                _logger.LogDebug("Deleting failed job: {id}", toDelete[i]);
                BackgroundJob.Delete(toDelete[i]);
            }

            _logger.LogInformation("Deleted {count} failed jobs.", toDelete.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting failed jobs");
            throw;
        }
    }

    public void DeleteFailedJobsWithoutNotifications()
    {
        _logger.LogInformation("Deleting failed jobs (without notifications)...");

        try
        {
            IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
            long count = monitor.FailedCount();

            if (count == 0)
            {
                _logger.LogDebug("No failed jobs to delete.");
                return;
            }

            int batch = _options.BatchSize;
            var pageCount = (int) ((count + batch - 1) / batch);
            var toDelete = new List<string>(Math.Min((int) count, 1000));

            for (var i = 0; i < pageCount; i++)
            {
                JobList<FailedJobDto>? jobs = monitor.FailedJobs(i * batch, batch);
                foreach ((string? key, _) in jobs)
                    toDelete.Add(key);
            }

            for (var i = 0; i < toDelete.Count; i++)
            {
                _logger.LogDebug("Deleting failed job: {id}", toDelete[i]);
                BackgroundJob.Delete(toDelete[i]);
            }

            _logger.LogInformation("Deleted {count} failed jobs.", toDelete.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting failed jobs");
            throw;
        }
    }

    public void DeleteSuccessfulJobs()
    {
        _logger.LogInformation("Deleting miscellaneous successful jobs...");

        IMonitoringApi? monitor = JobStorage.Current.GetMonitoringApi();
        long count = monitor.SucceededListCount();

        if (count == 0)
        {
            _logger.LogDebug("No successful jobs to delete.");
            return;
        }

        int batch = _options.BatchSize;
        var pageCount = (int) ((count + batch - 1) / batch);
        var toDelete = new List<string>(Math.Min((int) count, 1000));

        for (var i = 0; i < pageCount; i++)
        {
            JobList<SucceededJobDto>? jobs = monitor.SucceededJobs(i * batch, batch);
            foreach ((string? key, SucceededJobDto? job) in jobs)
            {
                if (_options.ShouldDeleteSucceededJob?.Invoke(job) ?? false)
                    toDelete.Add(key);
            }
        }

        for (var i = 0; i < toDelete.Count; i++)
        {
            _logger.LogDebug("Deleting successful job: {id}", toDelete[i]);
            BackgroundJob.Delete(toDelete[i]);
        }

        _logger.LogInformation("Deleted {count} successful jobs.", toDelete.Count);
    }

    public void DeleteExistingRecurringJobs()
    {
        using IStorageConnection? connection = JobStorage.Current.GetConnection();
        List<RecurringJobDto>? jobs = connection.GetRecurringJobs();

        for (var i = 0; i < jobs.Count; i++)
        {
            RecurringJobDto job = jobs[i];
            _logger.LogInformation("Removing recurring job: {id}", job.Id);
            RecurringJob.RemoveIfExists(job.Id);
        }
    }
}