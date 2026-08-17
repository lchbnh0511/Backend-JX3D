namespace BackendJX3D.Application.DTOs.Request.Chat;

public class SendChatRequest
{
    //Id kênh lấy từ GET /chat/channels
    public int ChannelId { get; set; }

    //Dạng "/tênNgườiNhận nội dung" thì thành chat riêng
    public string Message { get; set; } = string.Empty;
}
