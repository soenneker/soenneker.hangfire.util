using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Hangfire.Util.Abstract;

namespace Soenneker.Hangfire.Util.Registrars;

/// <summary>
/// Registers Hangfire storage cleanup operations.
/// </summary>
public static class HangfireUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IHangfireUtil"/> as a singleton service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddHangfireUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IHangfireUtil, HangfireUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IHangfireUtil"/> as a scoped service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddHangfireUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IHangfireUtil, HangfireUtil>();

        return services;
    }
}
