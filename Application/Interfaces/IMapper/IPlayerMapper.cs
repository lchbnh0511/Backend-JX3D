using BackendJX3D.Application.DTOs.Response.Player;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface IPlayerMapper
{
    PlayerResponse FromPlayerRequest(CURPLAYER_SYNC curPlayer, CURPLAYER_NORMAL_SYNC playerStats, string name, NPC_SYNC? playerNpc);
    PlayerSittingResponse FromSittingRequest(NPC_SIT_SYNC sit);
    PlayerRideResponse FromPlayerRideRequest(NPC_HORSE_SYNC horse);
    PlayerRunningResponse FromPlayerRunningRequest(NPC_RUN_SYNC run);
}
