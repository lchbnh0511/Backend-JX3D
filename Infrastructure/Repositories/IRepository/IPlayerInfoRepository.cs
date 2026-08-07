using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface IPlayerInfoRepository
{
    void AddOrUpdate(PlayerSyncInfo info);

    bool Remove(uint playerId);

    PlayerSyncInfo? Get(uint playerId);

    IReadOnlyCollection<PlayerSyncInfo> GetAll();

    bool Contains(uint playerId);

    int Count { get; }
}
