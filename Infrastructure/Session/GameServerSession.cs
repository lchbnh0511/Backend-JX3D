using System.Net;
using Network;
using Network.GameServer;

namespace BackendJX3D.Infrastructure.Session;

public class GameServerSession
{
    private static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(3);

    private CancellationTokenSource? _pingCts;

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

    public void StartPing()
    {
        StopPing();

        _pingCts = new CancellationTokenSource();

        _ = PingLoopAsync(_pingCts.Token);
    }

    public void StopPing()
    {
        _pingCts?.Cancel();
        _pingCts?.Dispose();
        _pingCts = null;
    }

    private async Task PingLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(PingInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                Console.WriteLine("Time: " + DateTime.Now);
                Client.Sender.SendPingPacket((uint)Environment.TickCount);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[GS PING] Dừng ping: {e.Message}");
        }
    }
}