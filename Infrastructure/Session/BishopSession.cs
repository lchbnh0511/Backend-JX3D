using Network.Header;

public class BishopSession
{
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