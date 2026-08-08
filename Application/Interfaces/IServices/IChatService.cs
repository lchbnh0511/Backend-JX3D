using BackendJX3D.Application.DTOs.Response.Chat;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IChatService
{
    Task<List<ChatChannelResponse>> GetChannels();

    Task<List<ChatResponse>> GetConversation(int limit = 20, int? channelId = null);

    Task<bool> SendMessage(int channelId, string message);
}
