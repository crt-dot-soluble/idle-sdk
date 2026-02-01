namespace IdleSdk.Core.Sandbox;

public sealed class RuntimeInspector
{
    private readonly Dictionary<string, RuntimeProperty> _properties = new(StringComparer.OrdinalIgnoreCase);

    private sealed class RuntimeProperty
    {
        public RuntimeProperty(RuntimePropertyDescriptor descriptor, Func<object> reader, Action<object>? writer)
        {
            Descriptor = descriptor;
            Reader = reader;
            Writer = writer;
        }

        public RuntimePropertyDescriptor Descriptor { get; }
        public Func<object> Reader { get; }
        public Action<object>? Writer { get; }
    }

    public void RegisterReader(string name, Func<object> reader)
    {
        RegisterProperty(name, typeof(object), reader, null, "");
    }

    public void RegisterProperty(string name, Type valueType, Func<object> reader, Action<object>? writer, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Property name must be provided.", nameof(name));
        }

        if (valueType is null)
        {
            throw new ArgumentNullException(nameof(valueType));
        }

        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var descriptor = new RuntimePropertyDescriptor(name, valueType, description ?? string.Empty, writer is not null);
        _properties[name] = new RuntimeProperty(descriptor, reader, writer);
    }

    public IReadOnlyCollection<RuntimePropertyDescriptor> ListProperties()
    {
        return _properties.Values.Select(prop => prop.Descriptor).ToList();
    }

    public object Read(string name)
    {
        if (!_properties.TryGetValue(name, out var property))
        {
            throw new KeyNotFoundException($"Property '{name}' was not found.");
        }

        return property.Reader();
    }

    public void Write(string name, object value)
    {
        if (!_properties.TryGetValue(name, out var property))
        {
            throw new KeyNotFoundException($"Property '{name}' was not found.");
        }

        if (property.Writer is null)
        {
            throw new InvalidOperationException($"Property '{name}' is read-only.");
        }

        property.Writer(value);
    }
}
