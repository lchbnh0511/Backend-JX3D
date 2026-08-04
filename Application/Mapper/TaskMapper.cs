using BackendJX3D.Application.DTOs.Response.Task;
using BackendJX3D.Application.Interfaces.IMapper;

namespace BackendJX3D.Application.Mapper;

public class TaskMapper : ITaskMapper
{
    public TaskResponse FromTaskRequest(KeyValuePair<ushort, int> task)
    {
        return new TaskResponse
        {
            TaskId = task.Key,
            TaskValue = task.Value,
        };
    }
}