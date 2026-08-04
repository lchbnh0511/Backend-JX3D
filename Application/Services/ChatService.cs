using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

namespace BackendJX3D.Application.Services;

public class ChatService : IChatService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;      
    private readonly IChatMapper _chatMapper;

    public ChatService(ISessionManager sessionManager,  ICurrentUser currentUser,  IChatMapper chatMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _chatMapper = chatMapper;
    }
    
    public async Task<List<ChatResponse>> GetConversation()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        var chat = session.Handler.State.Chats
            .GetAll()
            .Select(_chatMapper.FromChatRequest)
            .ToList();

        return await Task.FromResult(chat);
    }
}