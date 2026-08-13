using BackendJX3D.Application.DTOs.Response.Team;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Application.Mapper;

public class TeamMapper : ITeamMapper
{
    public TeamResponse FromTeamRequest(TeamSnapshot snapshot, uint selfId)
    {
        var response = new TeamResponse
        {
            HasTeam = snapshot.HasTeam,
            TeamServerId = snapshot.TeamServerId,
            CaptainId = snapshot.CaptainId,
            IsCaptain = snapshot.CaptainId != 0 && snapshot.CaptainId == selfId,
        };

        if (snapshot.Members != null)
        {
            foreach (var member in snapshot.Members)
            {
                response.Members.Add(new TeamMemberResponse
                {
                    Id = member.Id,
                    Name = member.Name ?? string.Empty,
                    Level = member.Level,
                    Faction = member.Faction,
                    Camp = member.Camp,
                    Portrait = member.Portrait,
                    LifePercent = member.LifePercent,
                    ManaPercent = member.ManaPercent,
                    MapX = member.MapX,
                    MapY = member.MapY,

                    // Id = 0 là chưa biết id, đừng so với 0 rồi kết luận là mình
                    IsCaptain = member.Id != 0 && member.Id == snapshot.CaptainId,
                    IsSelf = member.Id != 0 && member.Id == selfId,
                });
            }
        }

        if (snapshot.Invites != null)
        {
            foreach (var invite in snapshot.Invites)
            {
                response.Invites.Add(new TeamInviteResponse
                {
                    Idx = invite.Idx,
                    Name = invite.Name ?? string.Empty,
                });
            }
        }

        if (snapshot.Applicants != null)
            response.Applicants.AddRange(snapshot.Applicants);

        return response;
    }
}
