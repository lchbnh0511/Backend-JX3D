namespace BackendJX3D.Infrastructure.Session.Data;

/// <summary>
/// Thông tin chỉ người chơi mới có, gộp từ 2 packet cùng nội dung:
///   PLAYER_SYNC        - bản đầy đủ
///   PLAYER_NORMAL_SYNC - bản rút gọn, GS gửi liên tục
///
/// Tách khỏi NPC_SYNC vì hai tầng packet khác nhau, chỉ chung khoá ID.
/// </summary>
public struct PlayerSyncInfo
{
    public uint Id;

    /// <summary>Giá trị thô từ GS. >= 0 nghĩa là người này đang có tổ đội.</summary>
    public int TeamFactionInfo;

    public uint TongNameId;
    public string TongName;
    public byte PkFlag;
    public byte PkValue;
    public byte Translife;
    public int TitleId;

    public readonly bool HasTeam => TeamFactionInfo >= 0;
}
