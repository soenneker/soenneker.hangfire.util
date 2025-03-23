using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Hangfire.Util.Abstract;

namespace Soenneker.Hangfire.Util.Registrars;

/// <summary>
/// A general-purpose, reusable utility class for managing Hangfire background jobs
/// </summary>
public static class HangfireUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IHangfireUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddHangfireUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IHangfireUtil, HangfireUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IHangfireUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddHangfireUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IHangfireUtil, HangfireUtil>();

        return services;
    }
}
