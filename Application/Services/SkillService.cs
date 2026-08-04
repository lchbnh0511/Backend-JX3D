using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.DTOs.Response.Skill;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

namespace BackendJX3D.Application.Services;

public class SkillService : ISkillService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;      
    private readonly ISkillMapper  _skillMapper;

    public SkillService(ISessionManager sessionManager,  ICurrentUser currentUser, ISkillMapper  skillMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _skillMapper = skillMapper;
    }
    
    public async Task<List<SkillResponse>> GetListSkill()
    {
      var session = _sessionManager.Get(_currentUser.SessionId);
        
        var skills = session.Handler.State.Skills
            .GetAll()
            .Select(_skillMapper.FromSkillRequest)
            .ToList();
        
        return await Task.FromResult(skills);
    }
}