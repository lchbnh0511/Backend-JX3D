namespace BackendJX3D.Application.DTOs.Response.Item;

public class ItemMoveResponse
{
    public int ItemId { get; set; }

    
    public byte SrcPlace { get; set; }
    public byte SrcX { get; set; }
    public byte SrcY { get; set; }

    public byte DestPlace { get; set; }
    public byte DestX { get; set; }
    public byte DestY { get; set; }

    //Vật phẩm sau khi chuyển
    public ItemResponse? Item { get; set; }

    //Ảnh chụp hai kho sau khi chuyển, client khỏi gọi lại
    public List<ItemResponse> Inventory { get; set; } = [];
    public List<ItemResponse> Chest { get; set; } = [];
}
