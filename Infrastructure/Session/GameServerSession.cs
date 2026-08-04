using System.Net;
using Network;
using Network.GameServer;

namespace BackendJX3D.Infrastructure.Session;

public class GameServerSession
{
    public NetworkGSClient Client { get; set; }

    public Guid PlayerGuid { get; private set; }

    public GameServerSession(ProcessGame processGame)
    {
        Client = new NetworkGSClient(processGame);
    }
    
    
    public GS_ClientSend GetSender()
    {
        return Client.Sender;
    }

    public async Task ConnectAsync(
        uint ip,
        ushort port,
        Guid playerGuid)
    {
        PlayerGuid = playerGuid;

        Client.playerGuid = playerGuid;

        var address = new IPAddress(BitConverter.GetBytes(ip));

        await Client.ConnectAsync(
            address.ToString(),
            port);
    }
}