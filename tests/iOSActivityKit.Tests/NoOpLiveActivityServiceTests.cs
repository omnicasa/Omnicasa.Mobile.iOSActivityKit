using iOSActivityKit;
using Xunit;

namespace iOSActivityKit.Tests;

public class NoOpLiveActivityServiceTests
{
    private readonly ILiveActivityService service = new NoOpLiveActivityService();

    [Fact]
    public void IsSupported_is_false()
    {
        Assert.False(service.IsSupported);
    }

    [Fact]
    public void Start_returns_null()
    {
        var id = service.Start("sync", new Dictionary<string, string> { ["progress"] = "0" });
        Assert.Null(id);
    }

    [Fact]
    public void Update_End_EndAll_do_not_throw()
    {
        var ex = Record.Exception(() =>
        {
            service.Update("sync", new Dictionary<string, string> { ["progress"] = "0.5" });
            service.End(
                "sync",
                new Dictionary<string, string> { ["progress"] = "1" },
                TimeSpan.FromSeconds(5));
            service.End("sync");
            service.EndAll();
        });

        Assert.Null(ex);
    }
}
