namespace BackendJX3D.Infrastructure.Session.Data;


public sealed class PacketWaiter<T> where T : struct
{
    private TaskCompletionSource<T>? _tcs;

    /// <summary>Mở chỗ chờ. Phải gọi TRƯỚC khi gửi packet để không bị miss reply về sớm.</summary>
    public void Begin()
    {
        _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <summary>Recv thread gọi khi packet về. Không ai chờ thì bỏ qua.</summary>
    public void Complete(T data)
    {
        Interlocked.Exchange(ref _tcs, null)?.TrySetResult(data);
    }

    /// <summary>Chờ packet, trả null nếu quá hạn hoặc chưa Begin().</summary>
    public async Task<T?> WaitAsync(TimeSpan timeout)
    {
        var tcs = _tcs;

        if (tcs == null)
            return null;

        try
        {
            return await tcs.Task.WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            Interlocked.CompareExchange(ref _tcs, null, tcs);
            return null;
        }
    }
}
