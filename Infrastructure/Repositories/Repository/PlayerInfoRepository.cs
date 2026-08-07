using BackendJX3D.Infrastructure.Repositories.IRepository;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class PlayerInfoRepository : IPlayerInfoRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<uint, PlayerSyncInfo> _players = new();

    public void AddOrUpdate(PlayerSyncInfo info)
    {
        lock (_gate)
        {
            _players[info.Id] = info;
        }
    }

    public bool Remove(uint playerId)
    {
        lock (_gate)
        {
            return _players.Remove(playerId);
        }
    }

    public PlayerSyncInfo? Get(uint playerId)
    {
        lock (_gate)
        {
            return _players.TryGetValue(playerId, out var info) ? info : null;
        }
    }

    // Trả bản sao, không trả .Values (view sống - recv thread ghi giữa lúc caller duyệt là nổ)
    public IReadOnlyCollection<PlayerSyncInfo> GetAll()
    {
        lock (_gate)
        {
            return _players.Values.ToArray();
        }
    }

    public bool Contains(uint playerId)
    {
        lock (_gate)
        {
            return _players.ContainsKey(playerId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _players.Count;
            }
        }
    }
}
