using BackendJX3D.Core.Base;

namespace BackendJX3D.Infrastructure.Session.Data;

public sealed class PacketWaiter<T> where T : struct
{
    private TaskCompletionSource<T>? _tcs;

    /// <summary>
    /// Giữ chỗ chờ -> gửi lệnh -> chờ reply. Trả null nếu GS không phản hồi trong timeout.
    /// Ném ConflictException (409) nếu đang có lệnh cùng loại chưa xong.
    /// </summary>
    public async Task<T?> SendAndWaitAsync(Action send, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Chỉ giành được chỗ khi đang trống -> chống client spam command
        if (Interlocked.CompareExchange(ref _tcs, tcs, null) != null)
            throw new BaseException.ConflictException(
                "command_in_progress",
                "Lệnh trước chưa hoàn tất, thử lại sau.");

        try
        {
            // Gửi SAU khi đã giữ chỗ -> reply về sớm cỡ nào cũng không mất
            send();

            return await tcs.Task.WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            return null;
        }
        finally
        {
            // Nhả chỗ. Nếu thành công thì Complete() đã lấy ra rồi, CompareExchange không làm gì.
            Interlocked.CompareExchange(ref _tcs, null, tcs);
        }
    }

    /// <summary>Recv thread gọi khi packet về. Không ai chờ thì bỏ qua.</summary>
    public void Complete(T data)
    {
        Interlocked.Exchange(ref _tcs, null)?.TrySetResult(data);
    }
}
