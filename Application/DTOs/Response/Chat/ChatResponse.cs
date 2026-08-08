using Network.Header;

namespace BackendJX3D.Application.DTOs.Response.Chat;

public class ChatResponse
{
    //Thứ tự nhận trong phiên, tăng dần
    public long Seq { get; set; }

    //Id kênh GS cấp. -1 = tin hệ thống
    public int ChannelId { get; set; }

    public string ChannelName { get; set; } = string.Empty;

    public KProtocol.CHANNELRESOURCE? Channel { get; set; }

    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
