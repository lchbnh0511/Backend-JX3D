using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

public class BishopSession
{
    // Kết quả lệnh tạo/xoá nhân vật. null = chưa có phản hồi.
    public volatile RoleCommandResult? RoleCommand;

    public NetworkClient Client { get; set; }

    // public TaskCompletionSource<int> LoginTcs { get; }
    //     = new(TaskCreationOptions.RunContinuationsAsynchronously);
    //
    // public TaskCompletionSource<int> LoginServerTcs { get; }
    //     = new(TaskCreationOptions.RunContinuationsAsynchronously);
    //
    // public TaskCompletionSource<List<RoleBaseInfo>> CharacterTcs { get; }
    //     = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public List<RoleBaseInfo> Roles { get; } = new();
    public bool isLoadFullRoleBase = false;
    public int resultCode = -1;
    public int LoginServerResultCode = -1;

    public BishopSession(ProcessGame processGame)
    {
        Client = new NetworkClient(processGame);
    }
}