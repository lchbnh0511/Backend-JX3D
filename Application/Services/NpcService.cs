using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Application.Mapper;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

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
            .Where(x => x.ID != (uint)session.Handler.State.PlayerId)
            .Select(_npcMapper.FromNpcRequest)
            .ToList();

        return await Task.FromResult(npcs);
    }
}