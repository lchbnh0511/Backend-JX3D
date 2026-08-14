namespace BackendJX3D.Application.DTOs.Response.Npc;

public class NpcDialogResponse
{
    public bool HasDialog { get; set; }
    public uint NpcId { get; set; }
    public byte UiId { get; set; }

    //Số lựa chọn GS khai. So với Options.Count để biết cách tách nội dung có đúng chưa.
    public byte OptionNum { get; set; }

    //Đoạn đầu của nội dung - theo giả thiết là lời NPC
    public string Text { get; set; } = string.Empty;

    //Các đoạn sau đoạn đầu - theo giả thiết là danh sách lựa chọn
    public List<string> Options { get; set; } = [];

    //Toàn bộ đoạn đã tách, kể cả đoạn đầu. Để đối chiếu khi Text/Options tách chưa đúng.
    public List<string> Segments { get; set; } = [];

    //Tham số thô của gói, chưa dò được ý nghĩa
    public byte ByteParam1 { get; set; }
    public byte ByteParam2 { get; set; }
    public int Param { get; set; }
    public int Param1 { get; set; }
    public int Param2 { get; set; }
}
