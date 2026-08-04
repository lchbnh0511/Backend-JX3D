using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface IChatRepository
{
    void AddOrUpdate(CHANNEL_PI_MESSAGE_CHAT chat);

    bool Remove(int channelId);

    IReadOnlyList<CHANNEL_PI_MESSAGE_CHAT>? Get(int channelId);

    IReadOnlyCollection<CHANNEL_PI_MESSAGE_CHAT> GetAll();

    bool Contains(int channelId);

    int Count { get; }
}