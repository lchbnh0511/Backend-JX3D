using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Application.Mapper;

public class ChatMapper : IChatMapper
{
    public ChatResponse FromChatRequest(ChatMessage chat)
    {
        var name = chat.ChannelId >= 0 && ChatChannelRegistry.Instance.TryGetById((uint)chat.ChannelId, out var info)
            ? info.Name
            : string.Empty;

        return new ChatResponse
        {
            Seq = chat.Seq,
            ChannelId = chat.ChannelId,
            ChannelName = name,
            Channel = ChatChannel.FromName(name),
            Sender = chat.Sender ?? string.Empty,
            Message = chat.Message ?? string.Empty,
        };
    }
}
