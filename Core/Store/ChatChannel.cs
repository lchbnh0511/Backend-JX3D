using Network.Header;

namespace BackendJX3D.Core.Store;


public static class ChatChannel
{
    public static readonly string[] AllNames =
    [
        ChatSendFunctions.CH_NEARBY,    // \S
        ChatSendFunctions.CH_WORLD,     // global
        ChatSendFunctions.CH_CITY,      // \B
        ChatSendFunctions.CH_SYSTEM,    // GM
        ChatSendFunctions.CH_TEAM,      // \T
        ChatSendFunctions.CH_FACTION,   // \F
        ChatSendFunctions.CH_TONG,      // \O
    ];

    public static KProtocol.CHANNELRESOURCE? FromName(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (name == ChatSendFunctions.CH_NEARBY) return KProtocol.CHANNELRESOURCE.CH_NEARBY;
        if (name == ChatSendFunctions.CH_WORLD) return KProtocol.CHANNELRESOURCE.CH_WORLD;
        if (name == ChatSendFunctions.CH_CITY) return KProtocol.CHANNELRESOURCE.CH_CITY;
        if (name == ChatSendFunctions.CH_SYSTEM) return KProtocol.CHANNELRESOURCE.CH_SYSTEM;

        if (name.StartsWith(ChatSendFunctions.CH_TEAM)) return KProtocol.CHANNELRESOURCE.CH_TEAM;
        if (name.StartsWith(ChatSendFunctions.CH_FACTION)) return KProtocol.CHANNELRESOURCE.CH_FACTION;
        if (name.StartsWith(ChatSendFunctions.CH_TONG)) return KProtocol.CHANNELRESOURCE.CH_TONG;

        return null;
    }
}
