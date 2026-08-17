using Network.Header;

namespace BackendJX3D.Application.DTOs.Request.Item;

public class ItemMoveRequest
{
    public uint ItemId { get; set; }

    // Để trống thì hiểu là chuyển trong cùng kho hiện tại của vật phẩm.
    public ITEM_POSITION? DestPlace { get; set; }

    //Ô đích trong kho đó
    public byte DestX { get; set; }
    public byte DestY { get; set; }
}
