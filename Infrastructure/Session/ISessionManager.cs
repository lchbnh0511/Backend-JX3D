namespace BackendJX3D.Infrastructure.Session;

public interface ISessionManager
{
    void Add(string token, PlayerSession session);
    PlayerSession Get(string token);
    void Remove(string token);
}