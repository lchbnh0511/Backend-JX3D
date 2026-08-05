using System.Collections.Concurrent;

namespace BackendJX3D.Infrastructure.Session.Data;

public sealed class PacketWaiters
{
    /// <summary>
    /// Gửi lệnh rồi chờ packet loại T trả về. Null nếu GS không phản hồi trong timeout.
    /// Ném ConflictException (409) nếu đang có lệnh cùng loại chưa xong.
    /// </summary>
    public Task<T?> SendAndWaitAsync<T>(Action send, TimeSpan timeout) where T : struct
    {
        return Of<T>().SendAndWaitAsync(send, timeout);
    }

    /// <summary>Recv thread gọi khi packet về. Không ai chờ loại này thì bỏ qua, không tạo gì.</summary>
    public void Complete<T>(T data) where T : struct
    { 
        if (_waiters.TryGetValue(typeof(T), out var waiter))
            ((PacketWaiter<T>)waiter).Complete(data);
    }

    private readonly ConcurrentDictionary<Type, object> _waiters = new();

    private PacketWaiter<T> Of<T>() where T : struct
    {
        return (PacketWaiter<T>)_waiters.GetOrAdd(typeof(T), _ => new PacketWaiter<T>());
    }
}
