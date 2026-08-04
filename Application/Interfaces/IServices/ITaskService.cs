using BackendJX3D.Application.DTOs.Response.Task;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface ITaskService
{
    Task<List<TaskResponse>> GetListTask();
}