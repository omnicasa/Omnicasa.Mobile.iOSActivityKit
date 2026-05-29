using System.Text.Json;
using iOSActivityKit;
using Xunit;

namespace iOSActivityKit.Tests;

public class StateSerializerTests
{
    [Fact]
    public void Null_state_serializes_to_empty_object()
    {
        Assert.Equal("{}", StateSerializer.ToJson(null));
    }

    [Fact]
    public void Empty_state_serializes_to_empty_object()
    {
        Assert.Equal("{}", StateSerializer.ToJson(new Dictionary<string, string>()));
    }

    [Fact]
    public void Single_pair_is_a_valid_json_object()
    {
        var json = StateSerializer.ToJson(new Dictionary<string, string> { ["phase"] = "Downloading" });

        using var doc = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.Equal("Downloading", doc.RootElement.GetProperty("phase").GetString());
    }

    [Fact]
    public void All_pairs_are_preserved()
    {
        var state = new Dictionary<string, string>
        {
            ["progress"] = "0.42",
            ["phase"] = "Downloading",
            ["message"] = "12 MB of 30 MB",
        };

        var round = Parse(StateSerializer.ToJson(state));

        Assert.Equal(state.Count, round.Count);
        foreach (var pair in state)
        {
            Assert.Equal(pair.Value, round[pair.Key]);
        }
    }

    [Fact]
    public void Null_value_becomes_empty_string()
    {
        var state = new Dictionary<string, string> { ["k"] = null! };

        var round = Parse(StateSerializer.ToJson(state));

        Assert.Equal(string.Empty, round["k"]);
    }

    [Theory]
    [InlineData("quote\"inside")]
    [InlineData("back\\slash")]
    [InlineData("line\nbreak\tand tab")]
    [InlineData("emoji 🚀 and accents éàü")]
    [InlineData("</script>")]
    [InlineData("controlchar")]
    public void Special_characters_round_trip_intact(string value)
    {
        var json = StateSerializer.ToJson(new Dictionary<string, string> { ["v"] = value });

        // Must be parseable and decode back to the exact original value.
        var round = Parse(json);
        Assert.Equal(value, round["v"]);
    }

    [Fact]
    public void Special_characters_in_keys_round_trip_intact()
    {
        const string key = "weird \"key\" \\ \n";
        var round = Parse(StateSerializer.ToJson(new Dictionary<string, string> { [key] = "x" }));

        Assert.Equal("x", round[key]);
    }

    [Fact]
    public void Output_is_always_parseable_json()
    {
        var json = StateSerializer.ToJson(new Dictionary<string, string>
        {
            ["a"] = "1",
            ["b"] = "two",
        });

        var ex = Record.Exception(() => JsonDocument.Parse(json).Dispose());
        Assert.Null(ex);
    }

    private static Dictionary<string, string> Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var result = new Dictionary<string, string>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            result[prop.Name] = prop.Value.GetString() ?? string.Empty;
        }

        return result;
    }
}
