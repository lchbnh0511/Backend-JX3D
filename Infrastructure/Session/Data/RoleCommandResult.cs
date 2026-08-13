namespace BackendJX3D.Infrastructure.Session.Data;


public sealed class RoleCommandResult
{
    // Tên nhân vật do Bishop trả về, không phải tên mình gửi lên
    public string Name = string.Empty;

    public bool Succeeded;

    // Chỉ có nghĩa khi Succeeded = false
    public byte FailReason;
}
