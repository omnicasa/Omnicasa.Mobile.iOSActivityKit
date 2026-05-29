using System.Text;
using System.Text.Json;

namespace iOSActivityKit;

/// <summary>
/// Serializes the generic <c>string</c>→<c>string</c> activity state to the
/// compact JSON object the native bridge decodes. Lives in the shared library
/// (not the iOS-only code) so it is unit-testable on any host and stays trim-
/// and NativeAOT-safe (no reflection).
/// </summary>
internal static class StateSerializer
{
    /// <summary>
    /// Encodes <paramref name="state"/> as a JSON object of string values.
    /// Null values are written as empty strings; a null dictionary yields
    /// <c>"{}"</c>.
    /// </summary>
    public static string ToJson(IReadOnlyDictionary<string, string>? state)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            if (state is not null)
            {
                foreach (var pair in state)
                {
                    writer.WriteString(pair.Key, pair.Value ?? string.Empty);
                }
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
