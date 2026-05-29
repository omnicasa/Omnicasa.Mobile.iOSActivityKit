namespace iOSActivityKit;

/// <summary>
/// Default implementation used on every non-iOS platform (Android, Windows,
/// Mac Catalyst, …). All members are safe no-ops so callers don't need to
/// guard with platform checks.
/// </summary>
public sealed class NoOpLiveActivityService : ILiveActivityService
{
    /// <inheritdoc/>
    public bool IsSupported => false;

    /// <inheritdoc/>
    public string? Start(string name, IReadOnlyDictionary<string, string> state) => null;

    /// <inheritdoc/>
    public void Update(string name, IReadOnlyDictionary<string, string> state)
    {
    }

    /// <inheritdoc/>
    public void End(
        string name,
        IReadOnlyDictionary<string, string>? finalState = null,
        TimeSpan dismissAfter = default)
    {
    }

    /// <inheritdoc/>
    public void EndAll()
    {
    }
}
