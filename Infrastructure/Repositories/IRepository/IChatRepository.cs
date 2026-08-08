using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface IChatRepository
{
    //Số tin tối đa giữ lại cho MỖI kênh
    int CapacityPerChannel { get; }

    void Add(ChatMessage chat);

    //N tin gần nhất của mọi kênh, trộn theo đúng thứ tự thời gian. Cũ -> mới.
    IReadOnlyList<ChatMessage> GetRecent(int count);

    //N tin gần nhất của một channelId GS cấp. -1 = tin hệ thống. Cũ -> mới.
    IReadOnlyList<ChatMessage> GetRecentByChannelId(int count, int channelId);

    int Count { get; }
}
