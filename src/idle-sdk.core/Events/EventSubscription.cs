namespace IdleSdk.Core.Events;

public sealed class EventSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private bool _disposed;

    public EventSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _unsubscribe();
        _disposed = true;
    }
}
