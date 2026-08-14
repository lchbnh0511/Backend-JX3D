namespace BackendJX3D.Application.DTOs.Response.Player;

public class PlayerResponse
{
    public uint Id { get; set; }
    public bool IsSelf { get; set; }

    //IsSelf = true
    public PlayerInfoResponse?  PlayerInfo { get; set; }

    //IsSelf = true
    public PlayerStatsResponse? Stats { get; set; }
    
    public PlayerNearbyResponse? Visible { get; set; }
}
