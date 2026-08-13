using BackendJX3D.Application.DTOs.Response.Team;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

namespace BackendJX3D.Application.Services;

public class TeamService : ITeamService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;
    private readonly ITeamMapper _teamMapper;

    public TeamService(ISessionManager sessionManager, ICurrentUser currentUser, ITeamMapper teamMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _teamMapper = teamMapper;
    }

    public async Task<TeamResponse> GetTeam()
    {
        var state = _sessionManager.Get(_currentUser.SessionId).Handler.State;

        return await Task.FromResult(
            _teamMapper.FromTeamRequest(state.Team.GetSnapshot(), state.PlayerId));
    }

    public async Task<TeamResponse> CreateTeam()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        if (state.Team.GetSnapshot().HasTeam)
            throw new BaseException.ConflictException(
                "team_already_exists",
                "Đang ở trong một đội, giải tán hoặc rời đội trước khi tạo đội mới.");

        var sender = session.GameServer.GetSender();

        // Khoá theo id của mình: gói tạo đội GS trả về không mang id nào để đối chiếu,
        // mà mỗi phiên chỉ có một người tạo đội nên id mình là khoá đủ dùng.
        var result = await state.Waiters.SendAndWaitAsync<TeamCreateResult>(
            state.PlayerId,
            () => sender.SendApplyTeamCreatePacket(),
            GameCommand.Timeout);

        if (result == null)
            throw new BaseException.ErrorException(
                504,
                "gameserver_timeout",
                "Game server không phản hồi lệnh tạo đội.");

        if (!result.Value.Success)
            throw new BaseException.ErrorException(
                422,
                "team_create_rejected",
                $"Game server từ chối tạo đội, mã lỗi {result.Value.ErrorId}.");

        return _teamMapper.FromTeamRequest(state.Team.GetSnapshot(), state.PlayerId);
    }

    public async Task<TeamResponse> LeaveTeam()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        RequireTeam(state);

        var sender = session.GameServer.GetSender();

        // GS báo người rời đội bằng PLAYER_LEAVE_TEAM kèm npcId -> khoá chính là id mình
        var result = await state.Waiters.SendAndWaitAsync<PLAYER_LEAVE_TEAM>(
            state.PlayerId,
            () => sender.SendApplyTeamLeavePacket(),
            GameCommand.Timeout);

        if (result == null)
            throw new BaseException.ErrorException(
                504,
                "gameserver_timeout",
                "Game server không phản hồi lệnh rời đội.");

        return _teamMapper.FromTeamRequest(state.Team.GetSnapshot(), state.PlayerId);
    }

    public async Task<TeamResponse> KickMember(uint playerId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var team = RequireTeam(state);

        if (playerId == state.PlayerId)
            throw new BaseException.BadRequestException(
                "cannot_kick_self",
                "Không trục xuất chính mình, dùng lệnh rời đội.");

        RequireMember(team, playerId);
        RequireCaptain(team, state.PlayerId, "trục xuất thành viên");

        var sender = session.GameServer.GetSender();

        // Trục xuất và tự rời đội GS đều trả PLAYER_LEAVE_TEAM, khác nhau ở npcId trong gói
        var result = await state.Waiters.SendAndWaitAsync<PLAYER_LEAVE_TEAM>(
            playerId,
            () => sender.SendTeamKickMemberPacket(playerId),
            GameCommand.Timeout);

        if (result == null)
            throw new BaseException.ErrorException(
                504,
                "gameserver_timeout",
                "Game server không phản hồi lệnh trục xuất.");

        return _teamMapper.FromTeamRequest(state.Team.GetSnapshot(), state.PlayerId);
    }

    public async Task<TeamResponse> ChangeCaptain(uint playerId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var team = RequireTeam(state);

        if (playerId == state.PlayerId)
            throw new BaseException.BadRequestException(
                "already_captain_target",
                "Không nhường chức cho chính mình.");

        RequireMember(team, playerId);
        RequireCaptain(team, state.PlayerId, "nhường chức đội trưởng");

        var sender = session.GameServer.GetSender();

        var result = await state.Waiters.SendAndWaitAsync<PLAYER_TEAM_CHANGE_CAPTAIN>(
            playerId,
            () => sender.SendApplyTeamChangeCaptainPacket(playerId),
            GameCommand.Timeout);

        if (result == null)
            throw new BaseException.ErrorException(
                504,
                "gameserver_timeout",
                "Game server không phản hồi lệnh nhường chức đội trưởng.");

        return _teamMapper.FromTeamRequest(state.Team.GetSnapshot(), state.PlayerId);
    }

    public async Task<bool> DismissTeam()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var team = RequireTeam(state);

        RequireCaptain(team, state.PlayerId, "giải tán đội");

        session.GameServer.GetSender().SendApplyTeamDismissPacket();

        return await Task.FromResult(true);
    }

    public async Task<bool> InviteMember(uint playerId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        RequireOtherPlayer(state, playerId);

        // Lời mời đi tới máy người kia, GS không gửi gì về cho mình -> không chờ được
        session.GameServer.GetSender().SendApplyTeamInvitePacket(playerId);

        return await Task.FromResult(true);
    }

    public async Task<bool> JoinTeam(uint playerId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        RequireOtherPlayer(state, playerId);

        if (state.Team.GetSnapshot().HasTeam)
            throw new BaseException.ConflictException(
                "team_already_exists",
                "Đang ở trong một đội, rời đội trước khi xin vào đội khác.");

        // Đơn xin đi tới đội trưởng bên kia, GS không gửi gì về cho mình -> không chờ được.
        // Vào được đội hay không thì biết qua PLAYER_TEAM_ADD_MEMBER, client gọi lại GetTeam để thấy.
        session.GameServer.GetSender().SendClientCmdApplyAddTeamPacket(playerId);

        return await Task.FromResult(true);
    }

    private static TeamSnapshot RequireTeam(PlayerState state)
    {
        var team = state.Team.GetSnapshot();

        if (!team.HasTeam)
            throw new BaseException.ConflictException(
                "team_not_found",
                "Đang không ở trong đội nào.");

        return team;
    }

    private static void RequireMember(TeamSnapshot team, uint playerId)
    {
        foreach (var member in team.Members)
        {
            if (member.Id == playerId) return;
        }

        // Chặn ở đây thay vì gửi lên GS rồi chờ hết 3 giây timeout: GS bỏ qua lệnh
        // nhắm vào người ngoài đội mà không trả gói lỗi nào.
        throw new BaseException.NotFoundException(
            "member_not_found",
            $"Người chơi {playerId} không có trong đội.");
    }

    // CaptainId = 0 là chưa biết đội trưởng là ai (GS không gửi kèm danh sách thành viên),
    // lúc đó không chặn - để GS tự quyết, còn hơn chặn oan đội trưởng thật.
    private static void RequireCaptain(TeamSnapshot team, uint selfId, string action)
    {
        if (team.CaptainId == 0 || team.CaptainId == selfId) return;

        throw new BaseException.ErrorException(
            403,
            "not_team_captain",
            $"Chỉ đội trưởng mới được {action}.");
    }

    private static void RequireOtherPlayer(PlayerState state, uint playerId)
    {
        if (playerId == 0)
            throw new BaseException.BadRequestException(
                "player_invalid",
                "playerId không hợp lệ.");

        if (playerId == state.PlayerId)
            throw new BaseException.BadRequestException(
                "player_is_self",
                "Không thao tác với chính mình.");
    }
}
