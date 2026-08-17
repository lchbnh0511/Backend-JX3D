using Network.Header;

namespace BackendJX3D.Application.DTOs.Request.Item;

public class ItemMoveRequest
{
    public uint ItemId { get; set; }
    public ITEM_POSITION DestPlace { get; set; }
}
