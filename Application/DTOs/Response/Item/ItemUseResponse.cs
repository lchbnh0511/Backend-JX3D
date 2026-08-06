namespace BackendJX3D.Application.DTOs.Response.Item;

public class ItemUseResponse
{
    public int ItemId { get; set; }

    /// <summary>true = dùng hết, item không còn trong túi.</summary>
    public bool Removed { get; set; }

    /// <summary>Trạng thái mới của item sau khi dùng. Null khi Removed = true.</summary>
    public ItemResponse? Item { get; set; }
}
