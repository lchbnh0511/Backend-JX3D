using BackendJX3D.Application.DTOs.Response.Task;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface ITaskMapper
{
    TaskResponse FromTaskRequest(KeyValuePair<ushort, int> task);
}