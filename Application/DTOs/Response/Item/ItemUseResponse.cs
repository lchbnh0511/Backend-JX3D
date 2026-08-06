namespace BackendJX3D.Application.DTOs.Response.Item;

public class ItemUseResponse
{
    public int ItemId { get; set; }

    // Vị trí item lúc gửi lệnh - đúng bộ tham số đã bắn lên GS 
    public byte Place { get; set; }
    public byte DestPlace { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    
    public ItemResponse? Item { get; set; }
}
