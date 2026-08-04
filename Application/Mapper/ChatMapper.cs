using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IMapper;

namespace BackendJX3D.Application.Mapper;

public class ChatMapper : IChatMapper
{
    public ChatResponse FromChatRequest(CHANNEL_PI_MESSAGE_CHAT chat)
    {
        return new ChatResponse
        {
            Sender = chat.Sender,
            Message = chat.Message,
        };
    }
}