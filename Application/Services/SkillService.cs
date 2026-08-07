
using BackendJX3D.Application.DTOs.Response.Skill;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using Network.Header;

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

    public async Task<SkillPointResponse> UpdatePointSkill(int skillId, int points)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;
        var pointAvailable = state.CurPlayer?.m_wSkillPoint ?? 0;
        
        if (skillId < 0 || skillId > ushort.MaxValue || !state.Skills.Contains((ushort)skillId))
            throw new BaseException.BadRequestException("skillId_invalid", "Kỹ năng không hợp lệ.");

        if (points <= 0)
            throw new BaseException.BadRequestException("point_invalid", "Số điểm cộng phải lớn hơn 0.");

        if (points > pointAvailable)
            throw new BaseException.BadRequestException(
                "not_enough_skill_point",
                $"Không đủ điểm kỹ năng, còn {pointAvailable} điểm.");

        var data = await state.Waiters.SendAndWaitAsync<PLAYER_SKILL_LEVEL_SYNC>(
            skillId,
            () => session.GameServer.GetSender().SendPlayerAddSkillPointPacket(skillId, points),
            GameCommand.Timeout);
        
        if(data == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh cộng điểm kỹ năng, có thể lệnh bị từ chối.");

        return _skillMapper.FromSkillPointRequest(data.Value);
    }
}