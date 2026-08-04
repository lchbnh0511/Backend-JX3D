using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.Interfaces.IMapper;
using Network.Header;

namespace BackendJX3D.Application.Mapper;

public class ItemMapper : IItemMapper
{
    public ItemResponse FromItemRequest(ITEM_SYNC item)
    {
        return new ItemResponse
        {
            ItemID = item.m_dwID,
            State = item.m_nState,
            Nature = item.m_Nature,
            Genre = item.m_Genre,
            Detail = item.m_Detail,
            Particur = item.m_Particur,
            Series = item.m_Series,
            Level = item.m_Level,
            Place = item.m_btPlace,
            PosX = item.m_btX,
            Y = item.m_btY,
            Luck = item.m_Luck,
            MagicLevel = item.m_MagicLevel?.ToArray() ?? Array.Empty<int>(),
            Version = item.m_Version,
            Durability = item.m_Durability,
            RandomSeed = item.m_RandomSeed,
            Count = item.m_Count,
            ExpireTime = item.m_ExpireTime,
            Bind = item.m_Bind,
            Value = item.m_Value,
            Mantle = item.m_Mantle,
            Fortune = item.m_Fortune,
            EnhanceTimes = item.m_EnhanceTimes,
            SetPrice = item.m_SetPrice,
            dwStatus = item.m_dwStatus
        };
    }
    //
    // public List<ItemResponse> FromItemRequests(IEnumerable<ITEM_SYNC> items)
    // {
    //     return items.Select(FromItemRequest).ToList();
    // }
}