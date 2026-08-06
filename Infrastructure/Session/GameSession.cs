using BackendJX3D.Infrastructure.Session.Data;
using BackendJX3D.Domain.Entities;
using BackendJX3D.Core.Base;
using Network.Header;
using Network.Bishop;
using System.Net;
using Network;
using Network.Resource.Header;

namespace BackendJX3D.Infrastructure.Session;

public class GameSession : IEventHandler
{
    private BishopSession _bishop;
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
        ////Log($"{data}");
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

    public void OnSyncPlayer(PLAYER_SYNC data)
    {
         Log($"[OnSyncPlayer] {data.ID} ");
    }

    public void OnSyncPlayerMin(PLAYER_NORMAL_SYNC data)
    {
        //Log($"{data}");
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
    }

    public void OnSyncNpcMin(NPC_NORMAL_SYNC data)
    {
        
        if (data.ID == State.PlayerId) return;

        if (!State.Npcs.Contains(data.ID))
        {
            // Chưa có NPC đầy đủ -> yêu cầu server sync
            var gameServer = _gameServer();
            gameServer.Client.Sender.SendRequestNpcPacket(data.ID);
        }
        
        
        //
        // var npc = state.Npc;
        //
        // // Đã có thì chỉ update những field thay đổi
        // npc.m_CurrentLife = data.CurrentLife;
        // npc.m_CurrentWalkSpeed = data.WalkSpeed;
        // npc.m_CurrentRunSpeed = data.RunSpeed; 
        // npc.m_CurrentCastSpeed = data.CastSpeed;
    }

    public void OnSyncNpcMinPlayer(NPC_PLAYER_TYPE_NORMAL_SYNC data)
    {
        //Log($"{data}");
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
        //Log($"{data}");
    }

    public void OnNetCommandWalk(NPC_WALK_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnNetCommandRun(NPC_RUN_SYNC data)
    {
        Log("[OnNetCommandRun] ProtocolType " + data.ProtocolType + " ID " + data.ID + " nMpsX " + data.nMpsX + " nMpsY " + data.nMpsY);
        if (State.PlayerId != data.ID) return;
        State.Waiters.Complete(data);
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
        if (data.ID != State.PlayerId) return;

        State.Waiters.Complete(data);
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
        Log($"[OnNetCommandSetHorse] m_dwID "  + data.m_dwID + " m_bRideHorse " + data.m_bRideHorse);
        if (data.m_dwID != State.PlayerId) return;
        
        State.Waiters.Complete(data);
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
        //Log($"{data}");
    }

    public void Ons2cLevelUp(PLAYER_LEVEL_UP_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cGetCurAttribute(PLAYER_ATTRIBUTE_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cGetSkillLevel(PLAYER_SKILL_LEVEL_SYNC data)
    {
        //Log($"{data}");
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
        //Log($"{data}");
    }

    public void Ons2cUpdataSelfTeamInfo(PLAYER_SEND_SELF_TEAM_INFO data)
    {
        //Log($"{data}");
    }

    public void Ons2cApplyTeamInfoFalse(PLAYER_APPLY_TEAM_INFO_FALSE data)
    {
        //Log($"{data}");
    }

    public void Ons2cCreateTeam(PLAYER_SEND_CREATE_TEAM_SUCCESS data)
    {
        //Log($"{data}");
    }

    public void Ons2cApplyCreateTeamFalse(PLAYER_SEND_CREATE_TEAM_FALSE data)
    {
        //Log($"{data}");
    }

    public void Ons2cSetTeamState(PLAYER_TEAM_CHANGE_STATE data)
    {
        //Log($"{data}");
    }

    public void Ons2cApplyAddTeam(PLAYER_APPLY_ADD_TEAM data)
    {
        //Log($"{data}");
    }

    public void Ons2cTeamAddMember(PLAYER_TEAM_ADD_MEMBER data)
    {
        //Log($"{data}");
    }

    public void Ons2cLeaveTeam(PLAYER_LEAVE_TEAM data)
    {
        //Log($"{data}");
    }

    public void Ons2cTeamChangeCaptain(PLAYER_TEAM_CHANGE_CAPTAIN data)
    {
        //Log($"{data}");
    }

    public void Ons2cTeamInviteAdd(TEAM_INVITE_ADD_SYNC data)
    {
        //Log($"{data}");
    }

    public void Ons2cTeamMemberInfo(tagMemberInfo data)
    {
        //Log($"{data}");
    }

    public void Ons2cSyncItem(ITEM_SYNC data)
    {
        Log($"[Ons2cRemoveItem] ProtocolType " + data.ProtocolType +
            " m_btPlace " + data.m_btPlace + " m_Durability " + data.m_Durability + " randomSeed " + data.m_RandomSeed);
        
        _state.Items.AddOrUpdate(data);
    }

    public void Ons2cRemoveItem(ITEM_REMOVE_SYNC data)
    {
        Log($"[Ons2cRemoveItem] ProtocolType " + data.ProtocolType + " m_ID " + data.m_ID);

        _state.Items.Remove(data.m_ID);
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
        //Log($"{data}");
    }

    public void OnItemChangeDurability(ITEM_DURABILITY_CHANGE data)
    {
        //Log($"{data}");
    }

    public void OnOpenSaleBox(BUY_SELL_SYNC data)
    {
        //Log($"{data}");
    }

    public void OnOpenStoreBox(byte[] pMsg)
    {
        Log($"{pMsg}");
    }

    public void Ons2cSyncStoreItem(SSyncStoreItem data)
    {
        //Log($"{data}");
    }

    public void Ons2cViewStoreItem(SViewStoreItem data)
    {
        //Log($"{data}");
    }

    public void Ons2cBuyStoreItem(SBuyStoreItem data)
    {
        //Log($"{data}");
    }

    public void Ons2cOpenSuperShop(BUY_SELL_SYNC data)
    {
        //Log($"{data}");
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
        //Log($"{data}");
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
        //Log($"{data}");
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
        Log($"");
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
        State.Chats.AddOrUpdate(data);
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