using Network.Header;

namespace BackendJX3D.Application.DTOs.Request.Player;

public class PlayerAttributeRequest
{
    public UI_PLAYER_ATTRIBUTE Attribute { get; set; }

    public int Point { get; set; } = 1;
}
