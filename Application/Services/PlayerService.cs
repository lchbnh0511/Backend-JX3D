using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using Network.Header;

namespace BackendJX3D.Application.Services;

public class PlayerService : IPlayerService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;
    private readonly IPlayerMapper _playerMapper;
    
    private const uint TIME_WAIT_ASYNC = 3;

    public PlayerService(ISessionManager sessionManager, ICurrentUser currentUser, IPlayerMapper playerMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _playerMapper = playerMapper;
    }


    public async Task<PlayerResponse?> GetPlayer()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        if (state.CurPlayer == null || state.PlayerStats == null)
            return await Task.FromResult<PlayerResponse?>(null);

        var response = _playerMapper.FromPlayerRequest(state.CurPlayer.Value, state.PlayerStats.Value, state.Name!, state.PlayerNpc);

        return await Task.FromResult<PlayerResponse?>(response);
    }

    public async Task<PlayerSittingResponse> Sitting()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var sender = session.GameServer.GetSender();
        var waiters = session.Handler.State.Waiters;
        
        var data = await waiters.SendAndWaitAsync<NPC_SIT_SYNC>(
            session.Handler.State.PlayerId,
            () => sender.SendPlayerSitPacket(true),
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC));

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
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC));

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
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC));

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh Running.");

        return _playerMapper.FromPlayerRunningRequest(data.Value);
    }

    public async Task<PlayerAttributeResponse> UpdateAttributePoint(UI_PLAYER_ATTRIBUTE attribute, int point)
    {
        if (!Enum.IsDefined((UI_PLAYER_ATTRIBUTE)attribute))
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
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC));

        if (data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh cộng điểm tiềm năng, có thể lệnh bị từ chối.");

        return _playerMapper.FromPlayerAttributeRequest(data.Value);
    }
}
