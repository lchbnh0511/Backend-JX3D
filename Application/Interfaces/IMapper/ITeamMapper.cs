using BackendJX3D.Application.DTOs.Response.Team;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface ITeamMapper
{
    TeamResponse FromTeamRequest(TeamSnapshot snapshot, uint selfId);
}
