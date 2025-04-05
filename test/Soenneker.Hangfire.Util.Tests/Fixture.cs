using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.Fixtures.Unit;
using Soenneker.Hangfire.Util.Options;
using Soenneker.Hangfire.Util.Registrars;
using Soenneker.Utils.Test;

namespace Soenneker.Hangfire.Util.Tests;

public sealed class Fixture : UnitFixture
{
    public override System.Threading.Tasks.ValueTask InitializeAsync()
    {
        SetupIoC(Services);

        return base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        IConfiguration config = TestUtil.BuildConfig();
        services.AddSingleton(config);

        services.AddHangfireUtilAsScoped();

        services.Configure<HangfireUtilOptions>(options =>
        {
            options.BatchSize = 200;
            options.NotifyOnUnhandledFailedJobs = true;

            options.ShouldDeleteFailedJob = job =>
            {
                // Example: Only delete jobs older than 7 days
                return job.FailedAt < DateTime.UtcNow.AddDays(-7);
            };

            options.ShouldDeleteSucceededJob = job =>
            {
                // Example: Delete all successful jobs older than 1 day
                return job.SucceededAt < DateTime.UtcNow.AddDays(-1);
            };
        });
    }
}
