namespace BackendJX3D.Application.DTOs.Response.Npc;

public class NpcDialogResponse
{
    public bool HasDialog { get; set; }
    public uint NpcId { get; set; }
    public byte UiId { get; set; }

    public byte OptionNum { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    //Toàn bộ đoạn đã tách
    public List<string> Segments { get; set; } = [];
    
    public bool ShopOpened { get; set; }

    //Chỉ có khi ShopOpened = true
    public NpcShopResponse? Shop { get; set; }

    //tham số này ko biết là gì, để đại ở đây
    public byte ByteParam1 { get; set; }
    public byte ByteParam2 { get; set; }
    public int Param { get; set; }
    public int Param1 { get; set; }
    public int Param2 { get; set; }
}
