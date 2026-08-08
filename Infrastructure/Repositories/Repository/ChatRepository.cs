using BackendJX3D.Infrastructure.Repositories.IRepository;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class ChatRepository : IChatRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    // Mỗi kênh một hàng đợi riêng, trần riêng
    private readonly Dictionary<int, Queue<ChatMessage>> _byChannel = new();

    // Số thứ tự toàn cục -> trộn các hàng đợi lại vẫn đúng thứ tự thời gian
    private long _seq;

    public int CapacityPerChannel => 50;

    public void Add(ChatMessage chat)
    {
        lock (_gate)
        {
            chat.Seq = ++_seq;

            if (!_byChannel.TryGetValue(chat.ChannelId, out var queue))
            {
                queue = new Queue<ChatMessage>();
                _byChannel.Add(chat.ChannelId, queue);
            }

            queue.Enqueue(chat);

            while (queue.Count > CapacityPerChannel)
                queue.Dequeue();
        }
    }

    public IReadOnlyList<ChatMessage> GetRecent(int count)
    {
        if (count <= 0)
            return Array.Empty<ChatMessage>();

        lock (_gate)
        {
            return _byChannel.Values
                .SelectMany(q => q)
                .OrderBy(x => x.Seq)
                .TakeLast(count)
                .ToArray();
        }
    }

    public IReadOnlyList<ChatMessage> GetRecentByChannelId(int count, int channelId)
    {
        if (count <= 0)
            return Array.Empty<ChatMessage>();

        lock (_gate)
        {
            return _byChannel.TryGetValue(channelId, out var queue)
                ? queue.TakeLast(count).ToArray()
                : Array.Empty<ChatMessage>();
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _byChannel.Values.Sum(q => q.Count);
            }
        }
    }
}
