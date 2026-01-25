using System.Text.Json;

namespace IdleSdk.Core.Persistence;

public sealed class JsonStateSerializer<T> : IStateSerializer<T>
{
    private readonly JsonSerializerOptions _options;

    public JsonStateSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions { WriteIndented = true };
    }

    public string Serialize(T state) => JsonSerializer.Serialize(state, _options);

    public T Deserialize(string payload) => JsonSerializer.Deserialize<T>(payload, _options)
        ?? throw new InvalidOperationException("Failed to deserialize state payload.");
}
