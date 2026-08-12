using Network.Header;

namespace BackendJX3D.Core.Store;


public static class ChatChannel
{

    public static readonly string[] FixedNames =
    [
        ChatSendFunctions.CH_NEARBY,    // \S
        ChatSendFunctions.CH_WORLD,     // global
        ChatSendFunctions.CH_CITY,      // \B
        ChatSendFunctions.CH_SYSTEM,    // GM
    ];
    
    public static IEnumerable<string> DynamicNames(int teamId, int factionId, int tongId)
    {
        if (teamId >= 0) yield return ChatSendFunctions.CH_TEAM + teamId;
        if (factionId >= 0) yield return ChatSendFunctions.CH_FACTION + factionId;
        if (tongId > 0) yield return ChatSendFunctions.CH_TONG + tongId;
    }

    public static void SplitTeamFaction(int teamFactionInfo, out int teamId, out int factionId)
    {
        teamId = (short)(teamFactionInfo >> 16);
        factionId = (short)(teamFactionInfo & 0xFFFF);
    }

    //Toàn bộ tên đã đăng ký, dùng để dò lại id trong registry
    public static IEnumerable<string> QueriedNames(int teamId, int factionId, int tongId)
    {
        foreach (var name in FixedNames)
            yield return name;

        foreach (var name in DynamicNames(teamId, factionId, tongId))
            yield return name;
    }

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
