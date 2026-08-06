using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Application.Services;

public class ItemService : IITemService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;      
    private readonly IItemMapper _itemMapper;

    public ItemService(ISessionManager sessionManager,  ICurrentUser currentUser,  IItemMapper itemMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _itemMapper = itemMapper;
    }
    
    // public async Task<List<ItemResponse>> GetListItemByPlace(int nPlace, int type = 10)
    // {
    //     var session = _sessionManager.Get(_currentUser.SessionId);
    //
    //     var items = session.Handler.State.Items
    //         .GetByPlace((byte)nPlace)
    //         .Select(_itemMapper.FromItemRequest)
    //         .ToList();
    //
    //     return await Task.FromResult(items);
    // }
    
    public async Task<List<ItemResponse>> GetListItemByPlace(int nPlace, int type = 10)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        var items = session.Handler.State.Items
            .GetByPlace((byte)nPlace)
            .Where(x => type == 10 || x.m_Genre == type) 
            .Select(_itemMapper.FromItemRequest)
            .ToList();

        return await Task.FromResult(items);
    }

    public async Task<ItemUseResponse> UseItem(uint itemId, byte place, byte destPlace, byte x, byte y)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        session.GameServer.GetSender().SendPlayerUseItemPacket(itemId, place, destPlace, x, y);

        // State.Items do Ons2cSyncItem / Ons2cRemoveItem cập nhật từ recv thread
        var id = (int)itemId;
        var response = new ItemUseResponse
        {
            ItemId = id,
            Removed = !state.Items.Contains(id),
        };

        if (!response.Removed)
        {
            var item = state.Items.Get(id);

            if (item != null)
                response.Item = _itemMapper.FromItemRequest(item.Value);
        }

        return await Task.FromResult(response);
    }
}
