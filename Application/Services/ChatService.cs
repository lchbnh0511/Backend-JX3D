using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
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


    public async Task<List<ChatChannelResponse>> GetChannels()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var channels = new List<ChatChannelResponse>();

        var self = state.PlayerInfos.Get(state.PlayerId);

        var teamId = -1;
        var factionId = -1;
        var tongId = 0;

        if (self != null)
        {
            ChatChannel.SplitTeamFaction(self.Value.TeamFactionInfo, out teamId, out factionId);
            tongId = (int)self.Value.TongNameId;
        }

        foreach (var name in ChatChannel.QueriedNames(teamId, factionId, tongId))
        {
            var id = ChatChannelRegistry.Instance.GetChannelId(name);

            if (id == uint.MaxValue) continue;

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

        // "Tất cả" không phải một kênh của GS - không có key name nào cho nó, mình tự gom.
        // Biểu diễn bằng channelId bỏ trống. Còn global là kênh thế giới thật, lọc như mọi kênh khác.
        var messages = channelId == null
            ? chats.GetRecent(take)
            : chats.GetRecentByChannelId(take, channelId.Value);

        return await Task.FromResult(messages.Select(_chatMapper.FromChatRequest).ToList());
    }

 
    public async Task<bool> SendMessage(int channelId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new BaseException.BadRequestException("message_empty", "Nội dung chat rỗng.");

        var session = _sessionManager.Get(_currentUser.SessionId);
        var chatSend = session.GameServer.Client.chatSend;

        // Chat riêng đi đường khác hẳn: "/tên nội dung" -> SendSomeoneMessage.
        if (TryParseWhisperInput(message, out var target, out var whisper))
        {
            chatSend.SendSomeoneMessage(target, whisper);

            return await Task.FromResult(true);
        }

        if (channelId < 0)
            throw new BaseException.BadRequestException("channel_invalid", "channelId không hợp lệ.");

        if (!ChatChannelRegistry.Instance.TryGetById((uint)channelId, out var info))
            throw new BaseException.ErrorException(
                503,
                "channel_not_registered",
                $"channelId {channelId} chưa được GS cấp, gọi /chat/channels để lấy id hợp lệ.");

        chatSend.SendChannelMessageText(info.ChannelId, info.Cost, message);

        return await Task.FromResult(true);
    }

    private static bool TryParseWhisperInput(string text, out string target, out string message)
    {
        target = string.Empty;
        message = string.Empty;

        if (string.IsNullOrEmpty(text) || !text.StartsWith('/'))
            return false;

        var spaceIdx = text.IndexOf(' ');

        if (spaceIdx < 0)
            return false;

        target = text[1..spaceIdx].Trim();
        message = text[(spaceIdx + 1)..].Trim();

        return target.Length > 0 && message.Length > 0;
    }
}
