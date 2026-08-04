using BackendJX3D.Application.DTOs.Response.World;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

namespace BackendJX3D.Application.Services;

public class WorldService : IWorldService   
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;      

    public WorldService(ISessionManager sessionManager,  ICurrentUser currentUser)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
    }

    public Task<WorldResponse?> GetWorld()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var world = session.Handler.State.World;

        if (world == null)
            return Task.FromResult<WorldResponse?>(null);

        var response = new WorldResponse
        {
            SubWorldId = world.Value.SubWorld,
            Region = world.Value.Region,
            Weather = world.Value.Weather,
            Frame = world.Value.Frame,
            MapCopyID = world.Value.MapCopyID,
            szName = world.Value.GetName()
        };

        return Task.FromResult<WorldResponse?>(response);
    }
}