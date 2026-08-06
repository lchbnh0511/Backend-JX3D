using BackendJX3D.Application.DTOs.Response.Item;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IITemService
{
    Task<List<ItemResponse>> GetListItemByPlace(int nPlace, int type = 10);

    Task<ItemUseResponse> UseItem(uint itemId);

    Task<ItemUseResponse> UnEquipItem(uint itemId);
}