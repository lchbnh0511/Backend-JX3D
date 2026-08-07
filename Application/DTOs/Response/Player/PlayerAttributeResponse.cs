
using Network.Header;

namespace BackendJX3D.Application.DTOs.Response.Player;

public class PlayerAttributeResponse
{
    public UI_PLAYER_ATTRIBUTE Attribute { get; set; }

    public int BasePoint { get; set; }

    public int CurPoint { get; set; }

    public int LeavePoint { get; set; }
}
