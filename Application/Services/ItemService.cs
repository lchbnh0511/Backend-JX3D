using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

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
        var item = FindItem(itemId);

        if (item.m_btPlace == (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_already_equipped",
                "Vật phẩm đang được trang bị, dùng API tháo trang bị.");

        // destPlace = 0 -> server tự quyết: trang bị thì mặc lên, thuốc thì uống
        return await Task.FromResult(Send(itemId, item, destPlace: 0));
    }

    public async Task<ItemUseResponse> UnEquipItem(uint itemId)
    {
        var item = FindItem(itemId);

        if (item.m_btPlace != (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_not_equipped",
                "Vật phẩm không nằm trên người nên không tháo được.");

        return await Task.FromResult(Send(itemId, item, destPlace: (byte)ITEM_POSITION.pos_equiproom));
    }

    private ITEM_SYNC FindItem(uint itemId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var found = session.Handler.State.Items.Get((int)itemId);

        if (found == null)
            throw new BaseException.NotFoundException(
                "item_not_found",
                "Không tìm thấy vật phẩm này.");

        return found.Value;
    }

    private ItemUseResponse Send(uint itemId, ITEM_SYNC item, byte destPlace)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        session.GameServer.GetSender().SendPlayerUseItemPacket(itemId, item.m_btPlace, destPlace, item.m_btX, item.m_btY);

        return new ItemUseResponse
        {
            ItemId = (int)itemId,
            Place = item.m_btPlace,
            DestPlace = destPlace,
            X = item.m_btX,
            Y = item.m_btY,
            Item = _itemMapper.FromItemRequest(item),
        };
    }


    public async Task<bool> ThrowAwayItem(uint itemId)
    {
        var item = FindItem(itemId);
        
        //Check do dang mang
        if (item.m_btPlace == (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_already_equipped",
                "Vật phẩm đang được trang bị, tháo ra trước khi vứt.");

        var session = _sessionManager.Get(_currentUser.SessionId);

        session.GameServer.GetSender().SendPlayerThrowAwayItemPacket(itemId);

        return await Task.FromResult(true);
    }
}
