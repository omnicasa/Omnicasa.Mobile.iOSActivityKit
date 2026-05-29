using iOSActivityKit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iOSActivityKit.Tests;

public class FactoryAndDiTests
{
    // The test host is plain net9.0 (not iOS), so the factory must yield the
    // no-op implementation.
    [Fact]
    public void Create_returns_noop_on_non_ios_host()
    {
        Assert.IsType<NoOpLiveActivityService>(LiveActivity.Create());
    }

    [Fact]
    public void Current_is_non_null_and_unsupported()
    {
        Assert.NotNull(LiveActivity.Current);
        Assert.False(LiveActivity.Current.IsSupported);
    }

    [Fact]
    public void Current_is_cached()
    {
        Assert.Same(LiveActivity.Current, LiveActivity.Current);
    }

    [Fact]
    public void AddLiveActivities_registers_resolvable_singleton()
    {
        var provider = new ServiceCollection()
            .AddLiveActivities()
            .BuildServiceProvider();

        var first = provider.GetService<ILiveActivityService>();
        var second = provider.GetService<ILiveActivityService>();

        Assert.NotNull(first);
        Assert.Same(first, second);            // singleton
        Assert.IsType<NoOpLiveActivityService>(first);
    }
}
