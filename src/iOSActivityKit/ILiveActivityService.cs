namespace iOSActivityKit;

/// <summary>
/// Cross-platform entry point for driving iOS ActivityKit Live Activities
/// (Lock Screen + Dynamic Island) from .NET MAUI / .NET for iOS apps.
/// <para>
/// State is modeled as a flat string/string dictionary so a single prebuilt
/// native bridge works for any app. The matching widget extension reads the
/// same keys in SwiftUI. On non-iOS platforms every member is a safe no-op.
/// </para>
/// </summary>
public interface ILiveActivityService
{
    /// <summary>
    /// <c>true</c> when running on iOS 16.2+ and the user has Live Activities
    /// enabled for the app. Always <c>false</c> on other platforms.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Starts a Live Activity.
    /// </summary>
    /// <param name="name">
    /// Logical activity name (e.g. <c>"sync"</c>). Used to target later
    /// <see cref="Update"/>/<see cref="End"/> calls and lets the widget pick a
    /// layout via <c>context.attributes.name</c>.
    /// </param>
    /// <param name="state">Initial key/value state shown in the widget.</param>
    /// <returns>
    /// The activity id, or <c>null</c> if it could not be started (unsupported,
    /// disabled, or the per-app activity limit was reached).
    /// </returns>
    string? Start(string name, IReadOnlyDictionary<string, string> state);

    /// <summary>
    /// Updates every running activity with the given <paramref name="name"/>.
    /// </summary>
    void Update(string name, IReadOnlyDictionary<string, string> state);

    /// <summary>
    /// Ends every running activity with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Logical activity name to end.</param>
    /// <param name="finalState">
    /// Optional final frame to display; when <c>null</c> the last shown state is
    /// kept.
    /// </param>
    /// <param name="dismissAfter">
    /// How long the finished activity lingers before being removed. Use
    /// <see cref="TimeSpan.Zero"/> for the system default.
    /// </param>
    void End(
        string name,
        IReadOnlyDictionary<string, string>? finalState = null,
        TimeSpan dismissAfter = default);

    /// <summary>
    /// Immediately ends all Live Activities started through this library.
    /// </summary>
    void EndAll();
}
