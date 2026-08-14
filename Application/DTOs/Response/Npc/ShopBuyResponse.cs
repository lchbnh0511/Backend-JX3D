using BackendJX3D.Application.DTOs.Response.Item;

namespace BackendJX3D.Application.DTOs.Response.Npc;

public class ShopBuyResponse
{
    // true = túi đồ đã nhận thêm hàng sau khi gửi lệnh mua.
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int ShopIdx { get; set; }
    public int BuyIdx { get; set; }
    public int Count { get; set; }

    //Hàng thực nhận. Rỗng khi Success = false.
    public List<ShopBuyItemResponse> Items { get; set; } = [];

    //Chờ bao lâu mới thấy túi đổi
    public long WaitedMs { get; set; }
}

public class ShopBuyItemResponse
{
    public int AddedCount { get; set; }

    //true = món hoàn toàn mới trong túi, false = món đã có, chỉ tăng số lượng
    public bool IsNew { get; set; }

    public ItemResponse Item { get; set; } = null!;
}
