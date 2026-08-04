using System.Collections.Concurrent;
using BackendJX3D.Core.Base;

namespace BackendJX3D.Infrastructure.Session;

public class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<string, PlayerSession> _sessions = new();

    public void Add(string token, PlayerSession session)
    {
        _sessions[token] = session;
    }

    public PlayerSession Get(string token)
    {
        if (!_sessions.TryGetValue(token, out var session))
            throw new BaseException.NotFoundException("not_found", "Session không tồn tại");

        return session;
    }

    public void Remove(string token)
    {
        _sessions.TryRemove(token, out _);
    }
}