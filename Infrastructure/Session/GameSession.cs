using BackendJX3D.Infrastructure.Session.Data;
using BackendJX3D.Domain.Entities;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Core.Utils;
using Network.Header;
using Network.Bishop;
using System.Net;
using Network;
using Network.Resource.Header;

namespace BackendJX3D.Infrastructure.Session;

public class GameSession : IEventHandler
{
    private BishopSession _bishop;
    
    private bool _fixedChannelsQueried;
    private int _queriedTeamId = int.MinValue;
    private int _queriedFactionId = int.MinValue;
    private int _queriedTongId = int.MinValue;

    private const uint NpcRequestRetryMs = 5_000;

    private const int TeamSlots = 8;

    private readonly Dictionary<uint, uint> _npcRequestedAt = new();
    private Func<GameServerSession> _gameServer;
    private readonly PlayerState _state = new(); 
    public PlayerState State => _state;
    
    public void Initialize(BishopSession bishop, Func<GameServerSession> gameServer)
    {
        _bishop = bishop;
        _gameServer = gameServer;
    }
    
    public void Log(string message)
    {
        Console.WriteLine(message);
    }

    public async void ReturnNotifyClient(tagNotifyPlayerLogin data)
    {
        _bishop.LoginServerResultCode = data.nResult;

        if (data.nResult != BishopProtocolDef.ROLE_LOGIN_RESULT_SUCCESS)
        {
            _bishop.Client.Dispose();
            Log(GetLoginServerMessage(data.nResult));
            return;
        }

        var gameServer = _gameServer();

        gameServer.Client.playerGuid = data.guid;

        var ip = new IPAddress(BitConverter.GetBytes(data.nIPAddr));

        await gameServer.Client.ConnectAsync(ip.ToString(), data.wPort);
        
        gameServer.StartPing(); //  3s/1
    }
    
    
    public static string GetLoginServerMessage(int resultCode)
    {
        return resultCode switch
        {
            BishopProtocolDef.ROLE_LOGIN_RESULT_MAINTENANCE
                => "Server đang bảo trì.",

            BishopProtocolDef.ROLE_LOGIN_RESULT_IS_FULL
                => "Server đầy.",

            BishopProtocolDef.ROLE_LOGIN_RESULT_UNKNOWN
                => "Lỗi không xác định.",

            _ => "Lỗi không xác định"
        };
    }

    public void ResponseCreateCharacter(tagNewDelRoleResponse data)
    {
        var name = data.GetRoleName();

        Log($"[Bishop] kết quả tạo nhân vật '{name}': thành công={data.bSucceeded}, mã lỗi={data.cFailReason}");

        _bishop.RoleCommand = new RoleCommandResult
        {
            Name = name,
            Succeeded = data.bSucceeded,
            FailReason = data.cFailReason,
        };
    }

    public void HandlePacketByTypeOfBishop(NetworkClient client, Packet packet)
    {
        BS_ClientHandle.HandlePacket(client, packet);
    }

    public void HandlePacketByTypeOfGameServer(NetworkGSClient client, Packet packet)
    {
        ProtocolBufferProcessor.ProcessBuffer(client, packet.Payload);
    }

    public void ReturnResponseLogin(NetworkClient client, KLoginAccountInfo data)
    {
        int resultCode = data.Head.Param & ~BishopProtocolDef.LOGIN_ACTION_FILTER;

        Log($"Protocol: 0x{data.Head.cProtocol:X2}");
        Log($"Param   : 0x{data.Head.Param:X8}");
        Log($"resultCode  : {resultCode}");
        Log($"Account : {data.GetAccount()}");
        Log($"ulLastLoginTime     : {data.ulLastLoginTime}");
        Log($"ulLastLoginIP     : {data.ulLastLoginIP}");
        Log($"uLimitPlayTimeFlag     : {data.uLimitPlayTimeFlag}");
        Log($"uLimitOnlineSecond     : {data.uLimitOnlineSecond}");
        Log($"uGatewayID     : {data.uGatewayID}");
        Log($"szTokenPassword     : {data.GetTokenPassword()}");

        ReturnResultLoginInfo(resultCode);
    }

    public void ReturnRoleList(int index, RoleBaseInfo data, bool isLast, NetworkClient client)
    {
        if (_bishop.isLoadFullRoleBase)
        {
            _bishop.Roles.Clear();
            _bishop.isLoadFullRoleBase = false;
        }

        _bishop.Roles.Add(data);

        if(isLast)
        {
            _bishop.isLoadFullRoleBase = true;
        }
    }
    
    public void ReturnResultLoginInfo(int resultCode)
    {
        _bishop.resultCode = resultCode;

        if (resultCode == BishopProtocolDef.LOGIN_R_SUCCESS)
        {
            Log("Đăng nhập thành công!");
            return;
        }

        _bishop.Client.Dispose();

        Log(GetLoginMessage(resultCode));
    }

    public static string GetLoginMessage(int resultCode)
    {
        return resultCode switch
        {
            BishopProtocolDef.LOGIN_R_ACCOUNT_OR_PASSWORD_ERROR
                => "Sai tài khoản hoặc mật khẩu",

            BishopProtocolDef.LOGIN_R_ACCOUNT_EXIST
                => "Tài khoản đang đăng nhập",

            BishopProtocolDef.LOGIN_R_TIMEOUT
                => "Tài khoản hết thời gian chơi",

            BishopProtocolDef.LOGIN_R_FREEZE
                => "Tài khoản bị khóa",

            BishopProtocolDef.LOGIN_R_IS_FULL
                => "Server đầy",

            BishopProtocolDef.LOGIN_R_INVALID_PROTOCOLVERSION
                => "Sai phiên bản client",

            _ => "Lỗi không xác định"
        };
    }
    public void ReturnNoRoleList()
     { 
         _bishop.Roles.Clear(); 
         _bishop.isLoadFullRoleBase = true;
    }

    public void OnSyncCurPlayer(CURPLAYER_SYNC data)
    {
        //Log($"{data}");
        State.CurPlayer = data;
        Log("[OnSyncCurPlayer] " + data.m_dwID);
    }

    public void Ons2cSyncAllSkill(SKILL_SEND_ALL_SYNC_DATA data)
    {
        State.Skills.AddOrUpdate(data);
    }
    public void OnSyncCurNormalData(CURPLAYER_NORMAL_SYNC data)
    {
        State.PlayerStats = data;
    }

    public void OnSyncWorld(WORLD_SYNC data)
    {
        State.World = data;
    }

    private void QueryChatChannels(int teamFactionInfo, uint tongNameId)
    {
        ChatChannel.SplitTeamFaction(teamFactionInfo, out var teamId, out var factionId);

        var tongId = (int)tongNameId;

        var needFixed = !_fixedChannelsQueried;
        var needDynamic = teamId != _queriedTeamId
                          || factionId != _queriedFactionId
                          || tongId != _queriedTongId;

        if (!needFixed && !needDynamic) return;

        var chatSend = _gameServer().Client.chatSend;

        if (needFixed)
        {
            _fixedChannelsQueried = true;

            foreach (var name in ChatChannel.FixedNames)
                chatSend.SendQueryChannelByName(name);

            Log($"[Chat] Đăng ký {ChatChannel.FixedNames.Length} kênh cố định");
        }

        if (teamId >= 0 && teamId != _queriedTeamId)
        {
            _queriedTeamId = teamId;
            chatSend.SendQueryChannelByName(ChatSendFunctions.CH_TEAM + teamId);
            Log($"[Chat] Đăng ký kênh đội, teamId={teamId}");
        }

        if (factionId >= 0 && factionId != _queriedFactionId)
        {
            _queriedFactionId = factionId;
            chatSend.SendQueryChannelByName(ChatSendFunctions.CH_FACTION + factionId);
            Log($"[Chat] Đăng ký kênh môn phái, factionId={factionId}");
        }

        if (tongId > 0 && tongId != _queriedTongId)
        {
            _queriedTongId = tongId;
            chatSend.SendQueryChannelByName(ChatSendFunctions.CH_TONG + tongId);
            Log($"[Chat] Đăng ký kênh bang, tongId={tongId}");
        }
    }

    public void OnSyncPlayer(PLAYER_SYNC data)
    {
        var old = State.PlayerInfos.Get(data.ID);

        var info = new PlayerSyncInfo
        {
            Id = data.ID,
            TeamFactionInfo = data.TeamFactionInfo,
            TongNameId = data.dwTongNameID,
            TongName = data.GetTongName(),
            PkFlag = data.PKFlag,
            PkValue = data.PKValue,
            Translife = data.Translife,
            TitleId = data.TitleID,

            //gia tri ko chuan. lay gia tri dung o sync_min
            IsRidingHorse = old?.IsRidingHorse ?? false,
            HorseSeeded = old?.HorseSeeded ?? false,
        };

        State.PlayerInfos.AddOrUpdate(info);
    }
    

    public void OnSyncPlayerMin(PLAYER_NORMAL_SYNC data)
    {
        var old = State.PlayerInfos.Get(data.ID);

        //lay ten bang cu đỡ tốn 56byte/1 lần
        var tongName = old != null && old.Value.TongNameId == data.dwTongNameID
            ? old.Value.TongName
            : data.GetTongName();

        var seeded = old?.HorseSeeded ?? false;

        var riding = seeded
            ? old!.Value.IsRidingHorse
            : data.HorseType > 0;

        if (!seeded)
            Log($"[Horse] gieo mốc đầu id={data.ID} HorseType={data.HorseType} -> riding={riding}");

        State.PlayerInfos.AddOrUpdate(new PlayerSyncInfo
        {
            Id = data.ID,
            TeamFactionInfo = data.TeamFactionInfo,
            TongNameId = data.dwTongNameID,
            TongName = tongName,
            PkFlag = data.PKFlag,
            PkValue = data.PKValue,
            Translife = data.Translife,
            TitleId = data.TitleID,

            IsRidingHorse = riding,
            HorseSeeded = true,
        });

        if (data.ID == State.PlayerId)
            QueryChatChannels(data.TeamFactionInfo, data.dwTongNameID);
    }

    public void OnSyncNpc(NPC_SYNC data)
    {
        if (State.PlayerId == 0)
        {
            State.PlayerId = data.ID;
            State.Name = data.GetName();
            State.PlayerNpc = data;
            return;
        }

        // Server sync lại NPC của chính mình -> cập nhật toạ độ, không đẩy vào danh sách NPC
        if (data.ID == State.PlayerId)
        {
            State.PlayerNpc = data;
            return;
        }

        State.Npcs.AddOrUpdate(data);

        // Đã có bản đầy đủ -> thôi không xin nữa
        _npcRequestedAt.Remove(data.ID);
    }

    private void RequestNpcOnce(uint npcId)
    {
        var now = (uint)Environment.TickCount;

        if (_npcRequestedAt.TryGetValue(npcId, out var last) && now - last < NpcRequestRetryMs)
            return;

        _npcRequestedAt[npcId] = now;

        _gameServer().Client.Sender.SendRequestNpcPacket(npcId);
    }

    public void OnSyncNpcMin(NPC_NORMAL_SYNC data)
    {
        if (data.ID == State.PlayerId) return;

        var found = State.Npcs.Get(data.ID);

        if (found == null)
        {
            // Chưa có NPC đầy đủ -> xin server sync.
            RequestNpcOnce(data.ID);
            return;
        }

        // Đã có thì vá những field thay đổi liên tục, giữ nguyên phần chỉ NPC_SYNC mới có
        var npc = found.Value;

        npc.MapX = (uint)data.MapX;
        npc.MapY = (uint)data.MapY;
        npc.CurrentLife = data.CurrentLife;
        npc.CurrentLifeMax = data.CurrentLifeMax;
        npc.CurrentCamp = data.CurrentCamp;
        npc.MissionCamp = data.MissionCamp;
        npc.State = data.State;
        npc.m_CmdKind = data.m_CmdKind;
        npc.m_Param_X = data.m_Param_X;
        npc.m_Param_Y = data.m_Param_Y;
        npc.m_Param_Z = data.m_Param_Z;
        npc.WalkSpeed = data.WalkSpeed;
        npc.RunSpeed = data.RunSpeed;
        npc.AttackSpeed = data.AttackSpeed;
        npc.CastSpeed = data.CastSpeed;
        npc.m_dwStatus = data.m_dwStatus;

        State.Npcs.AddOrUpdate(npc);
    }

    public void OnSyncNpcMinPlayer(NPC_PLAYER_TYPE_NORMAL_SYNC data)
    {
        if (data.m_dwNpcID == State.PlayerId)
        {
            if (State.PlayerNpc is { } self)
            {
                self.MapX = data.m_dwMapX;
                self.MapY = data.m_dwMapY;
                State.PlayerNpc = self;
            }

            return;
        }

        // Gói toạ độ riêng cho NPC loại người chơi -> vá vị trí cho danh sách xung quanh
        var found = State.Npcs.Get(data.m_dwNpcID);

        if (found == null)
            return;

        var npc = found.Value;

        npc.MapX = data.m_dwMapX;
        npc.MapY = data.m_dwMapY;

        State.Npcs.AddOrUpdate(npc);
    }

    public void OnSyncObjectAdd(OBJ_ADD_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnSyncObjectState(OBJ_SYNC_STATE data)
    {
        //Log($"{data}");
    }

    public void OnSyncObjectDir(OBJ_SYNC_DIR data)
    {
        //Log($"{data}");
    }

    public void OnSyncObjectRemove(OBJ_SYNC_REMOVE data)
    {
        //Log($"{data}");
    }

    public void OnSyncMissleStatus(OBJ_MISSLE_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandRemoveNpc(NPC_REMOVE_SYNC data)
    {
        //Update kho
        State.Npcs.Remove(data.ID);
        State.PlayerInfos.Remove(data.ID);
        _npcRequestedAt.Remove(data.ID);
    }

    public void OnNetCommandWalk(NPC_WALK_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandRun(NPC_RUN_SYNC data)
    {
        Log("[OnNetCommandRun] ProtocolType " + data.ProtocolType + " ID " + data.ID + " nMpsX " + data.nMpsX + " nMpsY " + data.nMpsY);

        State.Waiters.Complete(data.ID, data);
    }

    public void OnNetCommandJump(NPC_JUMP_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandHurt(NPC_HURT_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandDeath(NPC_DEATH_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandSkillFailed(NPC_SKILL_FAILED_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandFlyChar(NPC_FLY_CHAR_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandSkill(NPC_SKILL_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandSit(NPC_SIT_SYNC data)
    {
        State.Waiters.Complete(data.ID, data);
    }

    public void OnNetCommandSetPos(NPC_PLAYER_TYPE_NORMAL_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNpcSleepSync(NPC_SLEEP_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandSetHorse(NPC_HORSE_SYNC data)
    {
        Log($"[Horse] set id={data.m_dwID} riding={data.m_bRideHorse}");

        var old = State.PlayerInfos.Get(data.m_dwID);

        if (old != null)
        {
            var info = old.Value;

            info.IsRidingHorse = data.m_bRideHorse;

            info.HorseSeeded = true;

            State.PlayerInfos.AddOrUpdate(info);
        }

        State.Waiters.Complete(data.m_dwID, data);
    }

    public void Ons2cNpcSetMenuState(NPC_SET_MENU_STATE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cNpcSetStoreName(NPC_SET_STORE_NAME_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cPlayerExp(PLAYER_EXP_SYNC data)
    {
        Log("[Ons2cPlayerExp] m_nExp " + data.m_nExp);
    }

    public void Ons2cLevelUp(PLAYER_LEVEL_UP_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cGetCurAttribute(PLAYER_ATTRIBUTE_SYNC data)
    {
        Log($"[Ons2cGetCurAttribute] m_btAttribute " + (UI_PLAYER_ATTRIBUTE)data.m_btAttribute +
            " m_nBasePoint " + data.m_nBasePoint +
            " m_nCurPoint " + data.m_nCurPoint +
            " m_nLeavePoint " + data.m_nLeavePoint);

        State.Attribute = data;

        // GS chưa chắc gửi lại CURPLAYER_SYNC -> tự trừ điểm tồn để lần gọi kế tiếp kiểm đúng
        if (State.CurPlayer is { } curAttr && data.m_nLeavePoint >= 0)
        {
            curAttr.m_wAttributePoint = (ushort)data.m_nLeavePoint;
            State.CurPlayer = curAttr;
        }

        State.Waiters.Complete(data.m_btAttribute, data);
    }

    public void Ons2cGetSkillLevel(PLAYER_SKILL_LEVEL_SYNC data)
    {
        Log($"[Ons2cGetSkillLevel] m_btSkillTemp " + data.m_btSkillTemp +
            " m_nSkillID " + data.m_nSkillID +
            " m_nSkillLevel " + data.m_nSkillLevel +
            " m_nAddLevel " + data.m_nAddLevel +
            " m_nSkillExp " + data.m_nSkillExp +
            " m_nNextSkillExp " + data.m_nNextSkillExp +
            " m_nLeavePoint " + data.m_nLeavePoint);
        
        var old = State.Skills.Get((ushort)data.m_nSkillID);

        if (old != null)
        {
            var skill = old.Value;

            skill.SkillLevel = (byte)data.m_nSkillLevel;
            skill.SkillExp = data.m_nSkillExp;
            skill.NextSkillExp = data.m_nNextSkillExp;
            skill.SkillTemp = data.m_btSkillTemp;

            State.Skills.AddOrUpdate(skill);
        }
        else
        {
            Log($"[Ons2cGetSkillLevel] skill {data.m_nSkillID} chưa có trong kho, bỏ qua cập nhật");
        }

        // GS chưa chắc gửi lại CURPLAYER_SYNC -> tự trừ điểm tồn để lần gọi kế tiếp kiểm đúng
        if (State.CurPlayer is { } curSkill && data.m_nLeavePoint >= 0)
        {
            curSkill.m_wSkillPoint = (ushort)data.m_nLeavePoint;
            State.CurPlayer = curSkill;
        }

        State.Waiters.Complete(data.m_nSkillID, data);
    }

    public void Ons2cSetFactionData(PLAYER_FACTION_DATA data)
    {
        //Log($"{data}");
    }

    public void Ons2cLeaveFaction(PLAYER_LEAVE_FACTION data)
    {
        //Log($"{data}");
    }

    public void OnPlayerRevive(NPC_REVIVE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cShowTeamInfo(PLAYER_SEND_TEAM_INFO data)
    {
        var team = State.Team.GetSnapshot();

        if (data.nTeamServerID != 0 && data.nTeamServerID == team.TeamServerId)
            State.Team.SetCaptain((uint)data.m_nCaptain);

        Log($"[Team] thông tin đội teamServerId={data.nTeamServerID} captain={data.m_nCaptain}");
    }

    // Danh sách thành viên đội mình: có npcId + tên, KHÔNG có máu/mana/toạ độ
    public void Ons2cUpdataSelfTeamInfo(PLAYER_SEND_SELF_TEAM_INFO data)
    {
        var members = new List<(uint Id, string Name)>(TeamSlots);

        if (data.m_dwNpcID != null && data.m_szNpcName != null)
        {
            for (var i = 0; i < TeamSlots && i < data.m_dwNpcID.Length; i++)
            {
                if (data.m_dwNpcID[i] == 0) continue;

                members.Add((data.m_dwNpcID[i], data.GetMemberName(i)));
            }
        }

        State.Team.SetRoster(data.nTeamServerID, members);

        Log($"[Team] danh sách đội teamServerId={data.nTeamServerID}, {members.Count} thành viên");

        
        State.Waiters.Complete(State.PlayerId, data);
    }

    public void Ons2cApplyTeamInfoFalse(PLAYER_APPLY_TEAM_INFO_FALSE data)
    {
        Log("[Team] GS từ chối yêu cầu xem thông tin đội");
    }

    public void Ons2cCreateTeam(PLAYER_SEND_CREATE_TEAM_SUCCESS data)
    {
        var self = string.IsNullOrEmpty(State.Name)
            ? Array.Empty<(uint, string)>()
            : new[] { (State.PlayerId, State.Name!) };

        State.Team.SetRoster(data.nTeamServerID, self);
        State.Team.SetCaptain(State.PlayerId);

        Log($"[Team] tạo đội thành công, teamServerId={data.nTeamServerID}");

        State.Waiters.Complete(State.PlayerId, new TeamCreateResult
        {
            Success = true,
            TeamServerId = data.nTeamServerID,
        });
    }

    public void Ons2cApplyCreateTeamFalse(PLAYER_SEND_CREATE_TEAM_FALSE data)
    {
        Log($"[Team] tạo đội thất bại, errorId={data.m_btErrorID}");

        State.Waiters.Complete(State.PlayerId, new TeamCreateResult
        {
            Success = false,
            ErrorId = data.m_btErrorID,
        });
    }

    public void Ons2cSetTeamState(PLAYER_TEAM_CHANGE_STATE data)
    {
        // chua thay xai
        Log($"[Team] đổi trạng thái đội state={data.m_btState} flag={data.m_btFlag}");
    }

    public void Ons2cApplyAddTeam(PLAYER_APPLY_ADD_TEAM data)
    {
        State.Team.AddApplicant(data.m_dwTarNpcID);

        Log($"[Team] có người xin vào đội, npcId={data.m_dwTarNpcID}");
    }

    public void Ons2cTeamAddMember(PLAYER_TEAM_ADD_MEMBER data)
    {
        var name = data.GetName();

        State.Team.AddMember(data.m_dwNpcID, name, data.m_btLevel);

        State.Team.RemoveApplicant(data.m_dwNpcID);

        Log($"[Team] thêm thành viên {data.m_dwNpcID} '{name}' cấp {data.m_btLevel}");

        State.Waiters.Complete(data.m_dwNpcID, data);
    }

    // Truc xuat or thoat team
    public void Ons2cLeaveTeam(PLAYER_LEAVE_TEAM data)
    {
        if (data.m_dwNpcID == State.PlayerId)
        {
            State.Team.Clear();
            Log("[Team] mình đã ra khỏi đội");
        }
        else
        {
            State.Team.RemoveMember(data.m_dwNpcID);
            Log($"[Team] thành viên {data.m_dwNpcID} đã ra khỏi đội");
        }

        State.Waiters.Complete(data.m_dwNpcID, data);
    }

    public void Ons2cTeamChangeCaptain(PLAYER_TEAM_CHANGE_CAPTAIN data)
    {
        State.Team.SetCaptain(data.m_dwCaptainID);

        Log($"[Team] đội trưởng mới={data.m_dwCaptainID} member={data.m_dwMemberID} flag={data.m_bFlag}");

        // Chưa xác minh được field nào là id mình đã gửi lên, nên báo theo cả hai.
        // Khoá nào không có ai chờ thì Complete là no-op, không tốn gì.
        State.Waiters.Complete(data.m_dwCaptainID, data);

        if (data.m_dwMemberID != data.m_dwCaptainID)
            State.Waiters.Complete(data.m_dwMemberID, data);
    }

    // Người khác mời mình vào đội. m_nIdx là số hiệu lời mời, cần giữ để trả lời đúng lời mời đó.
    public void Ons2cTeamInviteAdd(TEAM_INVITE_ADD_SYNC data)
    {
        var name = data.GetName();

        State.Team.AddInvite(data.m_nIdx, name);

        Log($"[Team] '{name}' mời vào đội, idx={data.m_nIdx}");
    }

    // Máu/mana/toạ độ của thành viên đội, KHÔNG có npcId -> ghép theo tên
    public void Ons2cTeamMemberInfo(tagMemberInfo data)
    {
        if (data.m_sTeamInfo == null) return;

        var members = new List<TeamMember>(TeamSlots);

        foreach (var info in data.m_sTeamInfo)
        {
            if (info.szName == null) continue;

            var name = info.GetName();

            if (string.IsNullOrEmpty(name)) continue;

            members.Add(new TeamMember
            {
                Name = name,
                Level = info.btLevel,
                Faction = info.cFaction,
                Camp = info.btCamp,
                Portrait = info.btPortrait,
                LifePercent = info.btLifePercent,
                ManaPercent = info.btManaPercent,
                MapX = info.nMpsX,
                MapY = info.nMpsY,
            });
        }

        State.Team.SetLiveInfo(members);
    }

    public void Ons2cSyncItem(ITEM_SYNC data)
    {
        Log($"[Ons2cRemoveItem] ProtocolType " + data.ProtocolType +
            " m_btPlace " + data.m_btPlace + " m_Durability " + data.m_Durability + " randomSeed " + data.m_RandomSeed);
        
        _state.Items.AddOrUpdate(data);

        _state.Waiters.Complete(data.m_dwID, new ItemChange { ItemId = data.m_dwID, Removed = false });
    }

    public void Ons2cRemoveItem(ITEM_REMOVE_SYNC data)
    {
        Log($"[Ons2cRemoveItem] ProtocolType " + data.ProtocolType + " m_ID " + data.m_ID);

        _state.Items.Remove(data.m_ID);

        _state.Waiters.Complete(data.m_ID, new ItemChange { ItemId = data.m_ID, Removed = true });
    }

    public void Ons2cSyncMoney(PLAYER_MONEY_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cMoveItem(PLAYER_MOVE_ITEM_SYNC data)
    {
        //Log($"{data}");
    }
    
    public void Ons2cItemAutoMove(ITEM_AUTO_MOVE_SYNC data)
    {
        var movedId = State.Items.MoveTo(data.m_btSrcPos, data.m_btSrcX, data.m_btSrcY, data.m_btDestPos, data.m_btDestX, data.m_btDestY);

        Log($"[Item] chuyển ({data.m_btSrcPos},{data.m_btSrcX},{data.m_btSrcY})"
            + $" -> ({data.m_btDestPos},{data.m_btDestX},{data.m_btDestY})"
            + $" itemId={movedId?.ToString() ?? "không thấy ở ô nguồn"}");

        if (movedId == null) return;

        // lấy ID làm key waiter
        State.Waiters.Complete(movedId.Value, data);
    }

    public void OnItemChangeDurability(ITEM_DURABILITY_CHANGE data)
    {
        //Log($"{data}");
    }
    
    public void OnOpenSaleBox(BUY_SELL_SYNC data)
    {
        var info = data.m_BuySellInfo;

        State.Shop = new ShopState
        {
            ShopIdx = info.m_nBuyIdx,
            MoneyUnit = info.m_nMoneyUnit,
            Tax = info.m_nTax,
            SubWorldId = info.m_SubWorldID,
            MapX = info.m_nMpsX,
            MapY = info.m_nMpsY,
        };

        Log($"[Shop] mở cửa hàng shopIdx={info.m_nBuyIdx} "
            + $"moneyUnit={info.m_nMoneyUnit} tax={info.m_nTax}");

        State.Waiters.Complete(State.PlayerId, data);
    }
    
    public void OnOpenStoreBox(byte[] pMsg)
    {
        // Chỉ log. Không ai chờ gói này: nội dung rương về bằng các gói ITEM_SYNC riêng
        // (m_btPlace = pos_exboxroom) nên GET /items/chest đọc kho là đủ.
        Log($"[Chest] mở rương, payload {pMsg?.Length ?? 0} byte");
    }

    public void Ons2cSyncStoreItem(SSyncStoreItem data)
    {
        Log("[Shop] Ons2cSyncStoreItem");
    }

    public void Ons2cViewStoreItem(SViewStoreItem data)
    {
        Log("[Shop] Ons2cViewStoreItem");
    }

    public void Ons2cBuyStoreItem(SBuyStoreItem data)
    {
        Log("[Shop] Ons2cBuyStoreItem");
    }

    public void Ons2cOpenSuperShop(BUY_SELL_SYNC data)
    {
        Log($"[Shop] Ons2cOpenSuperShop buyIdx={data.m_BuySellInfo.m_nBuyIdx} "
            + $"moneyUnit={data.m_BuySellInfo.m_nMoneyUnit} tax={data.m_BuySellInfo.m_nTax}");
    }

    public void Ons2cTradeChangeState(TRADE_CHANGE_STATE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cTradeMoneySync(TRADE_MONEY_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cTradeDecision(TRADE_DECISION_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cTradePressOkSync(TRADE_STATE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cTradeApplyStart(TRADE_APPLY_START_SYNC data)
    {
        //Log($"{data}");
    }


    public void OnSyncScriptAction(PLAYER_SCRIPTACTION_SYNC data)
    {
        var dialog = new NpcDialog
        {
            UiId = data.m_btUIId,
            OptionNum = data.m_btOptionNum,
            ByteParam1 = data.m_btParam1,
            ByteParam2 = data.m_btParam2,
            Param = data.m_nParam,
            Param1 = data.m_nParam1,
            Param2 = data.m_nParam2,

            // Không dùng data.GetContent(): nó cắt ở byte 0 đầu tiên nên mất hết lựa chọn
            Segments = ScriptContent.Split(data.m_pContent, data.m_nBufferLen),
        };

        State.Dialog = dialog;

        Log($"[Dialog] uiId={dialog.UiId} optionNum={dialog.OptionNum} "
            + $"segments={dialog.Segments.Length} bufferLen={data.m_nBufferLen}");

        State.Waiters.Complete(State.PlayerId, data);
    }

    public void Ons2cSyncRoleList(ROLE_LIST_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cShowMsg(SHOW_MSG_SYNC data, byte[] buffer)
    {
        Log("[Ons2cShowMsg] " + buffer.Length);
    }

    public void Ons2cDirectlyCastSkill(NPC_SKILL_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnSyncStateEffect(STATE_EFFECT_SYNC data, int nDataNum)
    {
        //Log($"{data}");
    }

    public void OnSyncStateClear(STATE_CLEAR_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cPing(PING_SERVERREPLY_COMMAND data)
    {
        Log("[Ons2cPing] ProtocolType " + data.ProtocolType + " m_dwTime " + data.m_dwTime + " m_dwReplyServerTime " + data.m_dwReplyServerTime);
        
    }

    public void OnServerReplyClientPing(PING_COMMAND data)
    {
        Log($"[OnServerReplyClientPing] PING_COMMAND " + data.m_dwTime);
    }

    public void OnRequestNpcFail(NPC_REQUEST_FAIL data)
    {
        // GS bác yêu cầu. Ghi lại mốc để RequestNpcOnce không xin lại ngay - không có dòng này
        // thì gói NPC_NORMAL_SYNC kế tiếp của id đó lại xin, thành vòng lặp vô hạn.
        _npcRequestedAt[data.ID] = (uint)Environment.TickCount;

        Log($"[Npc] GS từ chối yêu cầu NPC {data.ID}");
    }

    public void Ons2cChangeWeather(SYNC_WEATHER data)
    {
        //Log($"{data}");
    }

    public void Ons2cSyncTaskValue(S2C_SYNCTASKVALUE data)
    {
        State.Tasks.AddOrUpdate(data.nTaskId, data.nTaskValue);
    }

    public void Ons2cNotifyClient(tagNOTIFY_CLIENT data)
    {
        //Log($"{data}");
    }

    public void Ons2cQueryChannel()
    {
        // Không đăng ký ở đây: gói này có thể tới trước lúc GS thật sự sẵn sàng.
        // Việc đăng ký bám vào PLAYER_NORMAL_SYNC của chính mình.
        Log("[Chat] s2cQueryChannel");
    }

    public void Ons2cSetRunAttackTag(S2C_SETRUNATTACKTAG data)
    {
        //Log($"{data}");
    }

    public void Ons2cPKSyncNormalFlag(PK_NORMAL_FLAG_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cPKSyncEnmityState(PK_ENMITY_STATE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cPKSyncExerciseState(PK_EXERCISE_STATE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cPKValueSync(PK_VALUE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cViewItem(SViewItemInfo data)
    {
        //Log($"{data}");
    }

    public void Ons2cViewData(VIEW_EQUIP_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnLadderResult(LADDER_DATA data)
    {
        //Log($"{data}");
    }

    public void OnLadderList(LADDER_LIST data)
    {
        //Log($"{data}");
    }

    public void Ons2cSpringGame(SPRING_GAME_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cSpringGameResult(SPRING_GAME_RESULT_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cAuctionItem(PLAYER_AUCTION_ITEM_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cOpenAuction()
    {
        Log($"");
    }

    public void OnSyncNpcCustomTask(NPC_CUSTOM_TASK_SYNC dataNpcCustomTask, CUSTOM_TASK_VALUE[] dataCustomValue)
    {
        // throw new NotImplementedException();
    }

    public void Ons2cPartnerInfo(PARTNER_INFO_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnS2C_SyncWorldFrame(tagNotifyWorldFrame data)
    {
        //Log($"{data}");
    }

    public void OnS2C_NewTongHanlder()
    {
        Log($"");
    }

    public void OnS2C_TongBaseInfoSync(NPC_TONG_BASE_INFO_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnS2C_RegRelay()
    {
        Log($"");
    }

    public void Ons2cExtend()
    {
        Log($"");
    }

    public void Ons2cExtendChat(byte[] rawData)
    {
        var gameServer = _gameServer();
        gameServer.Client.ChatReceiver.ProcessExtendChat(rawData, State.Name);
    }

    public void OnHandlePIChannelChat(CHANNEL_PI_MESSAGE_CHAT data)
    {
        Log($"[Chat] nhận ChannelId={data.ChannelId} sender='{data.Sender}' msg='{data.Message}'");

        State.Chats.Add(new ChatMessage
        {
            ChannelId = data.ChannelId,
            Sender = data.Sender,
            Message = data.Message,
        });

    }

    public void Ons2cExtendFriend(byte[] data)
    {
        FriendService.HandleAllResponseFriend(data);
    }

    public void NotifyDisconnect(NetworkClient client)
    {
        // throw new NotImplementedException();
        Log($"NotifyDisconnect");
    }

    public void NotifyTimeout()
    {
        Log($"NotifyTimeout");
    }
}