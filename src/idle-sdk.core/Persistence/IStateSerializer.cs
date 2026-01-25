namespace IdleSdk.Core.Persistence;

public interface IStateSerializer<T>
{
    string Serialize(T state);
    T Deserialize(string payload);
}
