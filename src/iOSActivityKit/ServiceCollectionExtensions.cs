using Microsoft.Extensions.DependencyInjection;

namespace iOSActivityKit;

/// <summary>
/// DI registration helpers for <see cref="ILiveActivityService"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ILiveActivityService"/> as a singleton, resolving
    /// the iOS implementation on iOS and a no-op on every other platform.
    /// Call from <c>MauiProgram.CreateMauiApp</c>:
    /// <code>builder.Services.AddLiveActivities();</code>
    /// </summary>
    public static IServiceCollection AddLiveActivities(this IServiceCollection services)
    {
        services.AddSingleton<ILiveActivityService>(_ => LiveActivity.Create());
        return services;
    }
}
