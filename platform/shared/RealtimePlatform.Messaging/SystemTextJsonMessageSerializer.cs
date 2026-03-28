using System.Text.Json;

namespace RealtimePlatform.Messaging;

/// <summary>
/// JSON serializer for integration events using <see cref="JsonSerializer"/>.
/// </summary>
public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <inheritdoc />
    public ReadOnlyMemory<byte> Serialize(object message, Type runtimeType)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(runtimeType);
        return JsonSerializer.SerializeToUtf8Bytes(message, runtimeType, Options);
    }
}
