using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

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


    public async Task<List<ChatChannelResponse>> GetChannels()
    {
        var channels = new List<ChatChannelResponse>();

        foreach (var name in ChatChannel.AllNames)
        {
            var id = ChatChannelRegistry.Instance.GetChannelId(name);

            if (id == uint.MaxValue)
                continue;

            channels.Add(new ChatChannelResponse
            {
                ChannelName = name,
                ChannelId = (int)id,
            });
        }

        return await Task.FromResult(channels);
    }
    
    public async Task<List<ChatResponse>> GetConversation(int limit = 20, int? channelId = null)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var chats = session.Handler.State.Chats;

        var take = Math.Clamp(limit, 1, chats.CapacityPerChannel);

        var worldId = ChatChannelRegistry.Instance.GetChannelId(ChatSendFunctions.CH_WORLD);

        IReadOnlyList<ChatMessage> messages;

        if (channelId == null || (worldId != uint.MaxValue && channelId.Value == (int)worldId))
            messages = chats.GetRecent(take);
        else
            messages = chats.GetRecentByChannelId(take, channelId.Value);

        return await Task.FromResult(messages.Select(_chatMapper.FromChatRequest).ToList());
    }

 
    public async Task<bool> SendMessage(int channelId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new BaseException.BadRequestException("message_empty", "Nội dung chat rỗng.");

        if (channelId < 0)
            throw new BaseException.BadRequestException("channel_invalid", "channelId không hợp lệ.");

        if (!ChatChannelRegistry.Instance.TryGetById((uint)channelId, out var info))
            throw new BaseException.ErrorException(
                503,
                "channel_not_registered",
                $"channelId {channelId} chưa được GS cấp, gọi /chat/channels để lấy id hợp lệ.");

        var session = _sessionManager.Get(_currentUser.SessionId);

        session.GameServer.Client.chatSend.SendChannelMessageText(info.ChannelId, info.Cost, message);

        return await Task.FromResult(true);
    }
}
