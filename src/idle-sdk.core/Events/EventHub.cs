using System.Buffers;
using System.Collections.Concurrent;

namespace IdleSdk.Core.Events;

public sealed class EventHub
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly object _gate = new();

    public Action<Exception, object?>? HandlerException { get; set; }

    public EventSubscription Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => new List<Delegate>());
        lock (_gate)
        {
            handlers.Add(handler);
        }

        return new EventSubscription(() => Unsubscribe(handler));
    }

    public void Publish<TEvent>(TEvent eventData)
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        Delegate[]? snapshot = null;
        var count = 0;
        try
        {
            lock (_gate)
            {
                count = handlers.Count;
                if (count == 0)
                {
                    return;
                }
                snapshot = ArrayPool<Delegate>.Shared.Rent(count);
                handlers.CopyTo(snapshot, 0);
            }

            for (var i = 0; i < count; i++)
            {
                try
                {
                    ((Action<TEvent>)snapshot![i])(eventData);
                }
                catch (Exception ex)
                {
                    HandlerException?.Invoke(ex, eventData);
                }
            }
        }
        finally
        {
            if (snapshot is not null)
            {
                ArrayPool<Delegate>.Shared.Return(snapshot, true);
            }
        }
    }

    private void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        lock (_gate)
        {
            handlers.Remove(handler);
        }
    }
}
