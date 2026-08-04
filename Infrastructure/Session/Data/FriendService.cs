using System.Runtime.InteropServices;
using Network.GameServer;

public enum FriendEvent
{
    OnFriendListReceived,
    OnAddFriendResult,
    OnFriendInvite,
    OnFriendItemClicked,
}

namespace Network.Header
{
    // Phân loại quan hệ dựa trên Friendliness:
    //   > 0  => Hảo hữu / GoodFriend
    //   = 0  => Cừu nhân / LambPerson
    //   < 0  => Sổ đen / BlackList
    public enum FriendCategory
    {
        GoodFriend,   // Friendliness > 0
        LambPerson,  // Friendliness == 0
        BlackList     // Friendliness < 0
    }

    public static class FriendService
    {
        #region Constants & Data Types

        // Port của FRIEND_STATE_LIST (Network.dll không export enum này — xem KTongProtocol.h).
        public const byte StateOffline = 0;
        public const byte StateOnline = 1;
        public const byte StateEnemyOffline = 2;
        public const byte StateEnemyOnline = 3;
        public const byte StateDelete = 4;   // đối phương không còn tồn tại
        public const byte StateDismiss = 5;   // giải trừ quan hệ

        public static readonly List<FriendInfo> Friends =
            new List<FriendInfo>();

        // Tên bạn đời (port m_szMateName của KPlayer) — dùng để phân biệt "hủy phu thê".
        public static string MateName { get; private set; } = string.Empty;

        public static void SetMateName(string name) => MateName = name ?? string.Empty;

        public class FriendInfo
        {
            public string Unit;
            public string Group;
            public string Name;
            public byte State;
            public int Friendliness;
        }

        #endregion

        #region Core – Packet Handling

        public static void HandleAllResponseFriend(byte[] data)
        {
            if (data == null || data.Length < Marshal.SizeOf<tagBiProtoHeader>())
                return;

            var pExHdr = ProtocolHelper.ByteArrayToStructure<tagBiProtoHeader>(data);

            if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_s2c_relationship)
            {
                HandleFriendRelationship(data);
            }
            else if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_c2c_askaddfriend)
            {
                string name = ReadShortString(data, 8);
                NotifyInvite(name);
            }
            else if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_c2c_repaddfriend)
            {
                int answer = (data.Length >= 8) ? BitConverter.ToInt32(data, 4) : 0;
                string name = ReadShortString(data, 8);
                NotifyAddFriendResult(name, answer);
            }
            else if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_s2c_repsyncfriendlist)
            {
                var friends = ParseFriendList(data);
                AddFriends(friends);
            }
            else if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_s2c_friendstate)
            {
                HandleFriendState(data);
            }
            else if (pExHdr.cSubProtocol == (byte)FRIEND_PROTOCOL_ID.friend_s2c_syncassociate)
            {
                HandleFriendSyncAssociate(data);
            }
        }

        private static void HandleFriendRelationship(byte[] data)
        {
            var rel = ProtocolHelper.ByteArrayToStructure<FRIEND_STRUCT_RELATIONSHIP>(data);
            byte relation = rel.Relation;
            int friendliness = rel.FriendlinessOrParam;
            string name = ReadShortString(data, 9);   // PartnerName ở offset +9
            string unit = RelationUnitName(relation);

            if (relation == (byte)PLAYER_RELATIONSHIP.PLAYER_RELATION_NONE)
            {
                if (friendliness < 0)
                {
                    // Đối phương từ chối lời mời kết bạn.
                    NotifyAddFriendResult(name, 0);
                }
                else
                {
                    // Đối phương chủ động giải trừ quan hệ.
                    bool isMate = !string.IsNullOrEmpty(name) && name == MateName;
                    // string msg = isMate
                    //     ? $"{name} đã hủy quan hệ phu thê với bạn!"
                    //     : string.Format(SysMsgText.ChatDeletedFriend, name);
                    // PostFriendMsg(msg, SysMsgConfirm.Click, 1);

                    Remove(name);
                    if (isMate) SetMateName(string.Empty);
                }
            }
            else if (relation == (byte)PLAYER_RELATIONSHIP.PLAYER_RELATION_ENEMY)
            {
                AddOrUpdate(unit, "", name, StateEnemyOnline, friendliness);
            }
            else if (relation == (byte)PLAYER_RELATIONSHIP.PLAYER_RELATION_COUPLE)
            {
                SetMateName(name);
                AddOrUpdate(unit, "", name, StateOnline, friendliness);
            }
            else // PLAYER_RELATION_FRIEND / PLAYER_RELATION_BROTHER
            {
                // nFriendliness == 1 ⇒ vừa kết giao thành công; khác ⇒ chỉ là cập nhật độ thân mật.
                if (friendliness != 1)
                {
                    AddOrUpdate(unit, "", name, StateOnline, friendliness);
                }
                else
                {
                    // PostFriendMsg(string.Format(SysMsgText.ChatAddFriendSuccess, name),
                    //               SysMsgConfirm.Interview, 2);
                    AddOrUpdate(unit, "", name, StateOnline, friendliness);
                    NotifyAddFriendResult(name, 1);
                }
            }
        }

        private static void HandleFriendState(byte[] data)
        {
            var fs = ProtocolHelper.ByteArrayToStructure<FRIEND_STRUCT_FRIEND_STATE>(data);
            byte state = fs.State;

            int offset = Marshal.SizeOf<FRIEND_STRUCT_FRIEND_STATE>();
            while (offset < data.Length && data[offset] != 0)
            {
                string name = ReadCStringSeq(data, ref offset);
                if (string.IsNullOrEmpty(name)) continue;

                if (state == StateDelete || state == StateDismiss)
                {
                    Remove(name);
                    continue;
                }
                SetState(name, state);

                bool online = state == StateOnline || state == StateEnemyOnline;
                var f = Find(name);
                bool isEnemy = f != null && GetCategory(f) == FriendCategory.LambPerson;

                // string msg = isEnemy
                //     ? string.Format(online ? SysMsgText.ChatEnemyOnline : SysMsgText.ChatEnemyOffline, name)
                //     : string.Format(online ? SysMsgText.ChatFriendOnline : SysMsgText.ChatFriendOffline, name);
                // PostFriendMsg(msg, SysMsgConfirm.None, 0);
            }
        }


        private static void HandleFriendSyncAssociate(byte[] data)
        {
            int offset = Marshal.SizeOf<tagBiProtoHeader>();
            if (offset >= data.Length) return;

            string group = ReadCStringSeq(data, ref offset);
            // sParseUGName: tách theo '\n'; phần trước là "unit", phần sau (group) phải rỗng.
            int nl = group.IndexOf('\n');
            string unit = nl >= 0 ? group.Substring(0, nl) : string.Empty;

            while (offset < data.Length && data[offset] != 0)
            {
                string role = ReadCStringSeq(data, ref offset);
                if (!string.IsNullOrEmpty(role))
                    AddOrUpdate(unit, "", role, StateOnline, -1);
            }
        }

        #endregion

        #region Core – Friend Data & Notifications

        public static void AddFriends(List<FriendInfo> friends)
        {
            if (friends != null)
                Friends.AddRange(friends);

            // GameEventHandler.Invoke(FriendEvent.OnFriendListReceived);
        }

        // Thêm mới hoặc cập nhật một quan hệ theo tên (port FriendInfo/AddPeople).
        public static void AddOrUpdate(string unit, string group, string name, byte state, int friendliness)
        {
            if (string.IsNullOrEmpty(name)) return;

            var f = Find(name);
            if (f == null)
            {
                f = new FriendInfo { Name = name };
                Friends.Add(f);
            }
            f.Unit = unit;
            f.Group = group;
            f.State = state;
            f.Friendliness = friendliness;

            // GameEventHandler.Invoke(FriendEvent.OnFriendListReceived);
        }

        // Xóa một quan hệ theo tên (port RemovePeople).
        public static void Remove(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            // if (Friends.RemoveAll(x => x.Name == name) > 0)
            //     GameEventHandler.Invoke(FriendEvent.OnFriendListReceived);
        }

        // Cập nhật trạng thái online/offline (port FriendStatus).
        public static void SetState(string name, byte state)
        {
            var f = Find(name);
            if (f == null) return;
            f.State = state;
            //GameEventHandler.Invoke(FriendEvent.OnFriendListReceived);
        }

        public static void NotifyInvite(string name)
        {
            // if (!string.IsNullOrEmpty(name)) GameEventHandler.Invoke(FriendEvent.OnFriendInvite, name);
            // UIScreenService.Instance.RecieveInvite(InviteType.InviteFriend, name, 0);

        }

        public static void NotifyAddFriendResult(string name, int answer)
        {
            // if (!string.IsNullOrEmpty(name)) GameEventHandler.Invoke(FriendEvent.OnAddFriendResult, name, answer);
        }

        // Xóa quan hệ bạn bè: gửi lệnh lên server (unit lấy từ FriendInfo đang lưu).
        public static void DeleteFriend(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var f = Find(name);
            if (f == null) return;

            // Unit name theo loại quan hệ: Hảo hữu → LOC_G_FRIEND_UNITNAME, Cừu nhân → LOC_K_ENEMY_UNITNAME.
            string unit = GetCategory(f) == FriendCategory.GoodFriend
                ? GameDataDef.LOC_G_FRIEND_UNITNAME
                : GameDataDef.LOC_K_ENEMY_UNITNAME;

            NotifyDeleteFriend(unit, name);
        }

        public static void NotifyDeleteFriend(string unit, string playerName)
        {
            //GS_ClientSend.NotifyDeleteFriend(unit, playerName);
        }

        public static List<FriendInfo> GetByCategory(FriendCategory category)
        {
            var result = new List<FriendInfo>();
            for (int i = 0; i < Friends.Count; i++)
            {
                if (GetCategory(Friends[i]) == category)
                    result.Add(Friends[i]);
            }
            return result;
        }

        public static void Clear()
        {
            Friends.Clear();
        }

        #endregion

        #region Support & Utility (Parse / Helpers)

        public static List<FriendInfo> ParseFriendList(byte[] data)
        {
            var result = new List<FriendInfo>();

            int offset = Marshal.SizeOf<FRIEND_STRUCT_REP_SYNCFRIENDLIST>();

            string currentUnit = "";
            string currentGroup = "";

            while (offset < data.Length)
            {
                byte tag = data[offset++];

                if (tag == (byte)FriendSpecMarker.Over)
                    break;

                // ================= GROUP =================
                if (tag == (byte)FriendSpecMarker.Group)
                {
                    int start = offset;

                    // đọc string group (null-terminated)
                    while (offset < data.Length && data[offset] != 0)
                        offset++;

                    byte[] raw = new byte[offset - start];
                    Buffer.BlockCopy(data, start, raw, 0, raw.Length);
                    offset++; // skip \0

                    string decoded = Converter.DecodeBytes(raw);

                    // var split = decoded.Split('|');
                    var split = decoded.Split('\n');

                    if (split.Length >= 2)
                    {
                        currentUnit = split[0];
                        currentGroup = split[1];
                    }
                    else
                    {
                        currentGroup = "";
                        currentGroup = decoded;
                    }
                }

                // ================= ROLE =================
                else if (tag == (byte)FriendSpecMarker.Role)
                {
                    byte state = data[offset++];

                    int start = offset;

                    while (offset < data.Length && data[offset] != 0)
                        offset++;

                    byte[] nameRaw = new byte[offset - start];
                    Buffer.BlockCopy(data, start, nameRaw, 0, nameRaw.Length);
                    offset++; // skip

                    string name = Converter.DecodeBytes(nameRaw);

                    int friendliness = BitConverter.ToInt32(data, offset);
                    offset += 4;

                    result.Add(new FriendInfo
                    {
                        Unit = currentUnit,
                        Group = currentGroup,
                        Name = name,
                        State = state,
                        Friendliness = friendliness
                    });
                }

                else
                {
                    // unknown → break để tránh lệch buffer
                    break;
                }
            }

            return result;
        }

        private static string ReadShortString(byte[] data, int offset)
        {
            if (data == null || offset + 1 >= data.Length) return string.Empty;
            int start = offset + 1;            // bỏ qua byStrLen
            int end = start;
            while (end < data.Length && data[end] != 0) end++;
            if (end <= start) return string.Empty;

            byte[] raw = new byte[end - start];
            Buffer.BlockCopy(data, start, raw, 0, raw.Length);
            return Converter.DecodeBytes(raw);
        }

        // Đọc một chuỗi C (\0 kết thúc) bắt đầu tại offset, dời offset qua dấu \0.
        private static string ReadCStringSeq(byte[] data, ref int offset)
        {
            int start = offset;
            while (offset < data.Length && data[offset] != 0) offset++;

            byte[] raw = new byte[offset - start];
            Buffer.BlockCopy(data, start, raw, 0, raw.Length);

            if (offset < data.Length) offset++;   // bỏ qua \0
            return raw.Length > 0 ? Converter.DecodeBytes(raw) : string.Empty;
        }

        private static string RelationUnitName(byte relation)
        {
            switch ((PLAYER_RELATIONSHIP)relation)
            {
                case PLAYER_RELATIONSHIP.PLAYER_RELATION_FRIEND: return "Bằng hữu";
                case PLAYER_RELATIONSHIP.PLAYER_RELATION_COUPLE: return "Phu thê";
                case PLAYER_RELATIONSHIP.PLAYER_RELATION_BROTHER: return "Huynh đệ";
                case PLAYER_RELATIONSHIP.PLAYER_RELATION_ENEMY: return "Cừu nhân";
                default: return string.Empty;
            }
        }

        // Phát tin hệ thống loại "Bằng hữu" (port CoreDataChanged(GDCNI_SYSTEM_MESSAGE, ... SMT_FRIEND)).
        // private static void PostFriendMsg(string text, SysMsgConfirm confirm, byte priority)
        // {
        //     if (string.IsNullOrEmpty(text) || SysMsgCentre.Instance == null) return;
        //     SysMsgCentre.Instance.Post(new SystemMessage
        //     {
        //         Text = text,
        //         Type = SysMsgType.Friend,
        //         Confirm = confirm,
        //         Priority = priority
        //     });
        // }

        public static FriendInfo Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            for (int i = 0; i < Friends.Count; i++)
                if (Friends[i].Name == name) return Friends[i];
            return null;
        }

        public static FriendCategory GetCategory(FriendInfo friend)
        {
            if (friend.Friendliness > 0) return FriendCategory.GoodFriend;
            if (friend.Friendliness < 0) return FriendCategory.BlackList;
            return FriendCategory.LambPerson;
        }

        #endregion
    }
}
