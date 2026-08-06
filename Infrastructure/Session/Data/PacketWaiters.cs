using System.Collections.Concurrent;

namespace BackendJX3D.Infrastructure.Session.Data;

public sealed class PacketWaiters
{
    public Task<T?> SendAndWaitAsync<T>(long key, Action send, TimeSpan timeout, TimeSpan settle = default)
        where T : struct
    {
        return Of<T>().SendAndWaitAsync(key, send, timeout, settle);
    }

    public void Complete<T>(long key, T data) where T : struct
    {
        if (_waiters.TryGetValue(typeof(T), out var waiter))
            ((PacketWaiter<T>)waiter).Complete(key, data);
    }

    private readonly ConcurrentDictionary<Type, object> _waiters = new();

    private PacketWaiter<T> Of<T>() where T : struct
    {
        return (PacketWaiter<T>)_waiters.GetOrAdd(typeof(T), _ => new PacketWaiter<T>());
    }
}
