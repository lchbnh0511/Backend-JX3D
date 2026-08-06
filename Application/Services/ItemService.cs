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

    public async Task<ItemUseResponse> UseItem(uint itemId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;
        var id = (int)itemId;

        var found = state.Items.Get(id);

        if (found == null)
            throw new BaseException.NotFoundException(
                "item_not_found",
                "Không tìm thấy vật phẩm này.");

        var item = found.Value;

        session.GameServer.GetSender().SendPlayerUseItemPacket(itemId, item.m_btPlace, 0, item.m_btX, item.m_btY);

        return await Task.FromResult(new ItemUseResponse
        {
            ItemId = id,
            Place = item.m_btPlace,
            X = item.m_btX,
            Y = item.m_btY,
            Item = _itemMapper.FromItemRequest(item),
        });
    }
}
