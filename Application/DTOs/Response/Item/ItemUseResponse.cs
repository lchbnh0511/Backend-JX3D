namespace BackendJX3D.Application.DTOs.Response.Item;

public class ItemUseResponse
{
    public int ItemId { get; set; }

    // Vị trí item lúc gửi lệnh - đúng bộ tham số đã bắn lên GS
    public byte Place { get; set; }
    public byte DestPlace { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }

    public bool Removed { get; set; }

    //Trạng thái item sau khi GS xử lý xong. Null nếu Removed hoặc nếu GS cấp id mới
    public ItemResponse? Item { get; set; }

    // Ảnh chụp sau khi chùm packet đã lắng - client khỏi gọi lại, và đúng cả khi GS đổi id item
    public List<ItemResponse> Inventory { get; set; } = new();
    public List<ItemResponse> Equipment { get; set; } = new();
}
