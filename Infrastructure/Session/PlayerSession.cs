namespace BackendJX3D.Infrastructure.Session;

public class PlayerSession
{
    public BishopSession Bishop { get; set; }
    public GameServerSession GameServer { get; set; }
    public GameSession Handler { get; set; }
    
    
}