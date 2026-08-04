using BackendJX3D.Application.DTOs.Response.World;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IWorldService
{
    public Task<WorldResponse?> GetWorld(); 
}