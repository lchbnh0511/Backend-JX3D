namespace BackendJX3D.Application.DTOs.Response.Player;

public class PlayerInfoResponse
{
    public string m_sPlayerName { get; set; }
    public uint m_dwID { get; set; }
    public byte m_btLevel { get; set; }
    public bool m_bSex { get; set; }
    public byte m_btKind { get; set; }
    public byte m_btSeries { get; set; }
    public int m_wLifeMax { get; set; }
    public int m_wStaminaMax { get; set; }
    public int m_wManaMax { get; set; }
    public ushort m_wAttributePoint { get; set; }
    public ushort m_wSkillPoint { get; set; }
    public ushort m_wStrength { get; set; }
    public ushort m_wDexterity { get; set; }
    public ushort m_wVitality { get; set; }
    public ushort m_wEngergy { get; set; }
    public ushort m_wLucky { get; set; }
    public long m_nExp { get; set; }
    public long m_nNextLevelExp { get; set; }
    public byte m_btTranslife { get; set; }
    public byte m_byExchangeServer { get; set; }
    public byte m_byGameSvrIndex { get; set; }
    public byte m_byServerStatus { get; set; }
    public byte m_byReserve2 { get; set; }
    public byte m_btCurFaction { get; set; }
    public byte m_btFirstFaction { get; set; }
    public int m_nFactionAddTimes { get; set; }
    public ushort m_wServerID { get; set; }
    public ushort m_wEngergySetDamageV { get; set; }
    public int m_nApplyHorseAttrib { get; set; }
    public int m_nMoney1 { get; set; }
    public int m_nMoney2 { get; set; }
    public byte m_btEquipExpand { get; set; }
    public byte m_btExpandBox { get; set; }

    // Toạ độ lấy từ NPC_SYNC của chính mình
    public uint MapX { get; set; }
    public uint MapY { get; set; }
    public int Dir { get; set; }
}