namespace BackendJX3D.Infrastructure.Session.Data;


public struct TeamCreateResult
{
    public bool Success;

    // Chỉ có nghĩa khi Success = false. GS không kèm mô tả, chỉ có mã.
    public byte ErrorId;

    public uint TeamServerId;
}
