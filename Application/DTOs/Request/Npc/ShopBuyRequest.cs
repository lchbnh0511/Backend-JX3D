namespace BackendJX3D.Application.DTOs.Request.Npc;

public class ShopBuyRequest
{
    //Vị trí món hàng trong cửa hàng, app tra từ resource của nó. shopIdx backend tự giữ.
    public int BuyIdx { get; set; }

    public int Count { get; set; } = 1;
}
