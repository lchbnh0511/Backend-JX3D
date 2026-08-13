namespace BackendJX3D.Application.DTOs.Response.Account;

public class CreateCharacterResponse
{
    public string Name { get; set; } = string.Empty;

    //Giới tính (byRoleNo) và hệ (wPortraitID) đã dùng để tạo
    public byte Gender { get; set; }
    public byte Series { get; set; }

    //true = server đã tự cho vào game với nhân vật này
    public bool EnteredGame { get; set; }

    //Rỗng khi EnteredGame = true. Có nội dung khi server tự cho vào game nhưng thất bại.
    public string EnterGameMessage { get; set; } = string.Empty;

    // Danh sách nhân vật từ GET /characters KHÔNG chứa nhân vật vừa tạo khi EnteredGame = true.
    public bool CharacterListStale { get; set; }
}
