using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class ChatRepository : IChatRepository
{
    private readonly Dictionary<int, List<CHANNEL_PI_MESSAGE_CHAT>> _chats = new();

    public void AddOrUpdate(CHANNEL_PI_MESSAGE_CHAT chat)
    {
        if (!_chats.TryGetValue(chat.ChannelId, out var messages))
        {
            messages = new List<CHANNEL_PI_MESSAGE_CHAT>();
            _chats.Add(chat.ChannelId, messages);
        }

        messages.Add(chat);
    }

    public bool Remove(int channelId)
    {
        return _chats.Remove(channelId);
    }

    public IReadOnlyList<CHANNEL_PI_MESSAGE_CHAT>? Get(int channelId)
    {
        return _chats.TryGetValue(channelId, out var messages)
            ? messages
            : null;
    }

    public IReadOnlyCollection<CHANNEL_PI_MESSAGE_CHAT> GetAll()
    {
        return _chats.Values
            .SelectMany(messages => messages)
            .ToList();
    }

    public bool Contains(int channelId)
    {
        return _chats.ContainsKey(channelId);
    }

    public int Count => _chats.Values.Sum(messages => messages.Count);
}