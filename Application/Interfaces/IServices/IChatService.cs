using BackendJX3D.Application.DTOs.Response.Chat;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IChatService
{
    Task<List<ChatResponse>> GetConversation();
}