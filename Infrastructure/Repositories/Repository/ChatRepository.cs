using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class ChatRepository : IChatRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<int, List<CHANNEL_PI_MESSAGE_CHAT>> _chats = new();

    public void AddOrUpdate(CHANNEL_PI_MESSAGE_CHAT chat)
    {
        lock (_gate)
        {
            if (!_chats.TryGetValue(chat.ChannelId, out var messages))
            {
                messages = new List<CHANNEL_PI_MESSAGE_CHAT>();
                _chats.Add(chat.ChannelId, messages);
            }

            messages.Add(chat);
        }
    }

    public bool Remove(int channelId)
    {
        lock (_gate)
        {
            return _chats.Remove(channelId);
        }
    }

    // Trả bản sao, không trả List gốc (recv thread Add vào giữa lúc caller duyệt là nổ)
    public IReadOnlyList<CHANNEL_PI_MESSAGE_CHAT>? Get(int channelId)
    {
        lock (_gate)
        {
            return _chats.TryGetValue(channelId, out var messages)
                ? messages.ToArray()
                : null;
        }
    }

    public IReadOnlyCollection<CHANNEL_PI_MESSAGE_CHAT> GetAll()
    {
        lock (_gate)
        {
            return _chats.Values
                .SelectMany(messages => messages)
                .ToArray();
        }
    }

    public bool Contains(int channelId)
    {
        lock (_gate)
        {
            return _chats.ContainsKey(channelId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _chats.Values.Sum(messages => messages.Count);
            }
        }
    }
}
