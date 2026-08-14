using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

namespace BackendJX3D.Application.Services;

public class PlayerService : IPlayerService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;
    private readonly IPlayerMapper _playerMapper;
    

    public PlayerService(ISessionManager sessionManager, ICurrentUser currentUser, IPlayerMapper playerMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _playerMapper = playerMapper;
    }


    public async Task<PlayerResponse> GetPlayer(uint? playerId = null)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var targetId = playerId is null || playerId.Value == 0
            ? state.PlayerId
            : playerId.Value;

        var isSelf = targetId == state.PlayerId;

        var info = state.PlayerInfos.Get(targetId);
        
        var npc = isSelf ? state.PlayerNpc : state.Npcs.Get(targetId);

        if (!isSelf && npc == null && info == null)
            throw new BaseException.NotFoundException(
                "player_not_found",
                $"Chưa thấy người chơi {targetId}. Gọi GET /player/nearby để lấy danh sách quanh mình.");

        var response = new PlayerResponse
        {
            Id = targetId,
            IsSelf = isSelf,
        };


        if (isSelf && state.CurPlayer != null && state.PlayerStats != null)
        {
            var self = _playerMapper.FromPlayerRequest(
                state.CurPlayer.Value,
                state.PlayerStats.Value,
                state.Name ?? string.Empty,
                state.PlayerNpc);

            response.PlayerInfo = self.PlayerInfo;
            response.Stats = self.Stats;
        }

        if (npc != null)
            response.Visible = _playerMapper.FromPlayerNearbyRequest(npc.Value, info);

        return await Task.FromResult(response);
    }

    public async Task<PlayerSittingResponse> Sitting()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var sender = session.GameServer.GetSender();
        var waiters = session.Handler.State.Waiters;
        
        var data = await waiters.SendAndWaitAsync<NPC_SIT_SYNC>(
            session.Handler.State.PlayerId,
            () => sender.SendPlayerSitPacket(true),
            GameCommand.Timeout);

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh ngồi.");

        return _playerMapper.FromSittingRequest(data.Value);
    }

    public async Task<PlayerRideResponse> RideHorse()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var sender = session.GameServer.GetSender();
        var waiters = session.Handler.State.Waiters;
        
        var data = await waiters.SendAndWaitAsync<NPC_HORSE_SYNC>(
            session.Handler.State.PlayerId,
            () => sender.SendPlayerRidePacket(),
            GameCommand.Timeout);

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh RideHorse.");

        return _playerMapper.FromPlayerRideRequest(data.Value);
    }
    
    public async Task<PlayerRunningResponse> Running(int nDesX,  int nDesY)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var sender = session.GameServer.GetSender();
        var waiters = session.Handler.State.Waiters;
        
        var data = await waiters.SendAndWaitAsync<NPC_RUN_SYNC>(
            session.Handler.State.PlayerId,
            () => sender.SendPlayerRunPacket(nDesX, nDesY),
            GameCommand.Timeout);

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh Running.");

        return _playerMapper.FromPlayerRunningRequest(data.Value);
    }

    public async Task<PlayerAttributeResponse> UpdateAttributePoint(UI_PLAYER_ATTRIBUTE attribute, int point)
    {
        if (!Enum.IsDefined(attribute))
            throw new BaseException.BadRequestException("attribute_invalid", "Thuộc tính không hợp lệ.");

        if (point <= 0)
            throw new BaseException.BadRequestException("point_invalid", "Số điểm cộng phải lớn hơn 0.");

        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;
        var sender = session.GameServer.GetSender();

        var available = state.CurPlayer?.m_wAttributePoint ?? 0;

        if (point > available)
            throw new BaseException.BadRequestException("not_enough_attribute_point", $"Không đủ điểm tiềm năng, còn {available} điểm.");

        var data = await state.Waiters.SendAndWaitAsync<PLAYER_ATTRIBUTE_SYNC>(
            (byte)attribute,
            () => sender.SendApplyAddBaseAttributePacket((int)attribute, point),
            GameCommand.Timeout);

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh cộng điểm tiềm năng, có thể lệnh bị từ chối.");

        return _playerMapper.FromPlayerAttributeRequest(data.Value);
    }


    public async Task<List<PlayerNearbyResponse>> GetNearbyPlayers()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        var state = session.Handler.State;

        // Ghép 2 kho theo ID: Npcs cho vị trí/máu/tên, PlayerInfos cho tổ đội/bang hội
        var players = state.Npcs
            .GetAll()
            .Where(x => x.m_btKind == (byte)NPCKIND.kind_player)
            .Select(npc => _playerMapper.FromPlayerNearbyRequest(npc, state.PlayerInfos.Get(npc.ID)))
            .ToList();

        return await Task.FromResult(players);
    }
}
