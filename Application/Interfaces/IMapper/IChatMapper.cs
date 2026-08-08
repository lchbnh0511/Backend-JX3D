using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface IChatMapper
{
    ChatResponse FromChatRequest(ChatMessage chat);
}
