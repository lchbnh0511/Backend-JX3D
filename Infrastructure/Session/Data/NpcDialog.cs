namespace BackendJX3D.Infrastructure.Session.Data;


public sealed class NpcDialog
{
    // 0 = script tự tới chứ không do mình mở dialog (vd script nhiệm vụ).
    public uint NpcId;

    //SendClientCmdSelectUI: nSelectUi 
    public byte UiId;

    public byte OptionNum;

    public byte ByteParam1;
    public byte ByteParam2;

    public int Param;
    public int Param1;
    public int Param2;

    // Text NPC & Options
    public string[] Segments = [];
}
