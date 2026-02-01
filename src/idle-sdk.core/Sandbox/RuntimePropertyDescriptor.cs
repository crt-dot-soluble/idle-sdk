namespace IdleSdk.Core.Sandbox;

public sealed record RuntimePropertyDescriptor(
    string Name,
    Type ValueType,
    string Description,
    bool IsWritable);
