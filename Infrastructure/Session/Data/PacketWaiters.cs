using System.Collections.Concurrent;

namespace BackendJX3D.Infrastructure.Session.Data;


public sealed class PacketWaiters
{
    private readonly ConcurrentDictionary<Type, object> _waiters = new();

    /// <summary>Mở chỗ chờ cho packet loại T. Phải gọi TRƯỚC khi gửi lệnh.</summary>
    public void Begin<T>() where T : struct
    {
        Of<T>().Begin();
    }

    /// <summary>Recv thread gọi khi packet về. Không ai chờ loại này thì bỏ qua, không tạo gì.</summary>
    public void Complete<T>(T data) where T : struct
    {
        if (_waiters.TryGetValue(typeof(T), out var waiter))
            ((PacketWaiter<T>)waiter).Complete(data);
    }

    /// <summary>Chờ packet loại T, trả null nếu quá hạn.</summary>
    public Task<T?> WaitAsync<T>(TimeSpan timeout) where T : struct
    {
        return Of<T>().WaitAsync(timeout);
    }

    private PacketWaiter<T> Of<T>() where T : struct
    {
        return (PacketWaiter<T>)_waiters.GetOrAdd(typeof(T), _ => new PacketWaiter<T>());
    }
}
