namespace IdleSdk.Core.Sandbox;

public sealed class RuntimeInspector
{
    private readonly Dictionary<string, Func<object>> _readers = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterReader(string name, Func<object> reader)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Reader name must be provided.", nameof(name));
        }

        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        _readers[name] = reader;
    }

    public object Read(string name)
    {
        if (!_readers.TryGetValue(name, out var reader))
        {
            throw new KeyNotFoundException($"Reader '{name}' was not found.");
        }

        return reader();
    }
}
