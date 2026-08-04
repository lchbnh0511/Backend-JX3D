using BackendJX3D.Application.DTOs.Response.Item;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface IItemMapper
{
    public ItemResponse FromItemRequest(ITEM_SYNC item);
}