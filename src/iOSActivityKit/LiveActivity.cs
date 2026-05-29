namespace iOSActivityKit;

/// <summary>
/// Convenience entry point for apps that don't use dependency injection.
/// Resolves the correct <see cref="ILiveActivityService"/> for the running
/// platform.
/// </summary>
public static class LiveActivity
{
    private static ILiveActivityService? current;

    /// <summary>
    /// Shared singleton service for the current platform.
    /// </summary>
    public static ILiveActivityService Current => current ??= Create();

    /// <summary>
    /// Creates a new platform-specific <see cref="ILiveActivityService"/>.
    /// Returns the iOS implementation on iOS and a no-op everywhere else.
    /// </summary>
    public static ILiveActivityService Create()
    {
#if IOS
        return new iOSActivityKit.Platforms.iOS.LiveActivityServiceiOS();
#else
        return new NoOpLiveActivityService();
#endif
    }
}
