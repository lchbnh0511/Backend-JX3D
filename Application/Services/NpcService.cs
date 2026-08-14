using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Application.Mapper;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using Network.Header;

namespace BackendJX3D.Application.Services;

public class NpcService : INpcService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;
    private readonly INpcMapper  _npcMapper;

    public NpcService(ISessionManager sessionManager, ICurrentUser currentUser, INpcMapper  npcMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _npcMapper = npcMapper;
    }

    public async Task<List<NpcResponse>> GetListNpc()
    {

        var session = _sessionManager.Get(_currentUser.SessionId);

        var npcs = session.Handler.State.Npcs
            .GetAll()
            .Select(_npcMapper.FromNpcRequest)
            .ToList();

        return await Task.FromResult(npcs);
    }

    public async Task<NpcDialogResponse> OpenDialog(uint npcId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        if (state.Npcs.Get(npcId) == null)
            throw new BaseException.NotFoundException(
                "npc_not_found",
                $"Không thấy NPC {npcId} quanh đây. Gọi GET /npc để lấy danh sách.");

        var sender = session.GameServer.GetSender();

        state.Dialog = null;

        var packet = await state.Waiters.SendAndWaitAsync<PLAYER_SCRIPTACTION_SYNC>(
            state.PlayerId,
            () => sender.SendNpcDialogPacket((int)npcId),
            GameCommand.Timeout);

        if (packet == null)
            return _npcMapper.FromDialogRequest(null, npcId);

        var dialog = state.Dialog;

        if (dialog != null)
            dialog.NpcId = npcId;

        return _npcMapper.FromDialogRequest(dialog, npcId);
    }

    public async Task<NpcDialogResponse> SelectDialogOption(int index)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var current = state.Dialog;

        if (current == null)
            throw new BaseException.ConflictException(
                "no_dialog_open",
                "Không có dialog nào đang mở. Gọi POST /npc/dialog trước.");

        if (index < 0)
            throw new BaseException.BadRequestException(
                "option_invalid",
                "index không hợp lệ.");

        var npcId = current.NpcId;
        var uiId = current.UiId;

        var protocol = session.GameServer.Client.Protocol;

        state.Dialog = null;

        var packet = await state.Waiters.SendAndWaitAsync<PLAYER_SCRIPTACTION_SYNC>(
            state.PlayerId,
            () => protocol.SendClientCmdSelectUI(index, uiId),
            GameCommand.Timeout);

        if (packet == null)
            return _npcMapper.FromDialogRequest(null, npcId);

        var next = state.Dialog;

        if (next != null)
            next.NpcId = npcId;

        return _npcMapper.FromDialogRequest(next, npcId);
    }
}
