using BackendJX3D.Core.Base;

namespace BackendJX3D.Infrastructure.Session.Data;

public sealed class PacketWaiter<T> where T : struct
{
    private sealed class Pending
    {
        public readonly TaskCompletionSource<T> Anchor = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Mỗi packet cùng nhóm về sau khi đã neo -> Release() để reset đồng hồ lắng
        public readonly SemaphoreSlim Pulse = new(0);

        public bool Anchored;
    }

    private readonly object _gate = new();

    private readonly Dictionary<long, Pending> _pending = new();
    
    public async Task<T?> SendAndWaitAsync(long key, Action send, TimeSpan timeout, TimeSpan settle)
    {
        var pending = new Pending();

        lock (_gate)
        {
            if (_pending.ContainsKey(key))
                throw new BaseException.ConflictException(
                    "command_in_progress",
                    "Lệnh trước trên đối tượng này chưa xong, thử lại sau.");

            _pending[key] = pending;
        }

        try
        {
            // Gửi SAU khi đã giữ chỗ -> reply về sớm cỡ nào cũng không mất
            send();

            T anchor;

            try
            {
                anchor = await pending.Anchor.Task.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }

            // Chờ chùm packet lắng xuống trước khi cho caller đọc State
            if (settle > TimeSpan.Zero)
            {
                while (await pending.Pulse.WaitAsync(settle))
                {
                }
            }

            return anchor;
        }
        finally
        {
            lock (_gate)
            {
                if (_pending.TryGetValue(key, out var current) && current == pending)
                    _pending.Remove(key);
            }
        }
    }

    public void Complete(long key, T data)
    {
        lock (_gate)
        {
            Pending? justAnchored = null;

            if (_pending.TryGetValue(key, out var pending) && !pending.Anchored)
            {
                pending.Anchored = true;
                justAnchored = pending;

                pending.Anchor.TrySetResult(data);
            }

            // Packet tiếp theo của cùng chùm có thể mang key khác (vd mặc đồ: item cũ ra, item mới vào)
            // -> reset đồng hồ lắng cho mọi lệnh đã neo.
            foreach (var other in _pending.Values)
            {
                if (other.Anchored && other != justAnchored)
                    other.Pulse.Release();
            }
        }
    }
}
