using BackendJX3D.Application.DTOs.Response.Task;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

namespace BackendJX3D.Application.Services;

public class TaskService : ITaskService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;      
    private readonly ITaskMapper  _taskMapper;

    public TaskService(ISessionManager sessionManager, ICurrentUser currentUser, ITaskMapper  taskMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _taskMapper = taskMapper;
    }


    public async Task<List<TaskResponse>> GetListTask()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        
        var tasks = session.Handler.State.Tasks
            .GetAll()
            .Select(_taskMapper.FromTaskRequest)
            .ToList();

        return await Task.FromResult(tasks);
    }
}