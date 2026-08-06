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
    
    
    private const double TIME_WAIT_ASYNC = 2;
    private static readonly TimeSpan ItemSettle = TimeSpan.FromMilliseconds(200);

    public ItemService(ISessionManager sessionManager,  ICurrentUser currentUser,  IItemMapper itemMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _itemMapper = itemMapper;
    }
    
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

        return await SendAndConfirm(itemId, item, destPlace: 0, "Game server không phản hồi lệnh dùng vật phẩm.");
    }

    public async Task<ItemUseResponse> UnEquipItem(uint itemId)
    {
        var item = FindItem(itemId);

        if (item.m_btPlace != (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_not_equipped",
                "Vật phẩm không nằm trên người nên không tháo được.");

        return await SendAndConfirm(itemId, item, (byte)ITEM_POSITION.pos_equiproom,
            "Game server không phản hồi lệnh tháo trang bị.");
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
    
    private async Task<ItemUseResponse> SendAndConfirm(uint itemId, ITEM_SYNC item, byte destPlace, string timeoutMessage)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var change = await state.Waiters.SendAndWaitAsync<ItemChange>(
            itemId,
            () => session.GameServer.GetSender()
                .SendPlayerUseItemPacket(itemId, item.m_btPlace, destPlace, item.m_btX, item.m_btY),
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC),
            ItemSettle);

        if (change == null)
            throw new BaseException.ErrorException(504, "Gameserver_timeout", timeoutMessage + " Có thể lệnh bị từ chối.");

        // Chùm packet đã lắng -> State là nguồn đúng, không suy diễn từ packet
        var after = state.Items.Get((int)itemId);

        return new ItemUseResponse
        {
            ItemId = (int)itemId,
            Place = item.m_btPlace,
            DestPlace = destPlace,
            X = item.m_btX,
            Y = item.m_btY,
            Removed = after == null,
            Item = after == null ? null : _itemMapper.FromItemRequest(after.Value),
            Inventory = Snapshot(state, ITEM_POSITION.pos_equiproom),
            Equipment = Snapshot(state, ITEM_POSITION.pos_equip),
        };
    }

    private List<ItemResponse> Snapshot(PlayerState state, ITEM_POSITION place)
    {
        return state.Items
            .GetByPlace((byte)place)
            .Select(_itemMapper.FromItemRequest)
            .ToList();
    }
    
    public async Task<bool> ThrowAwayItem(uint itemId)
    {
        var item = FindItem(itemId);

        if (item.m_btPlace == (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_already_equipped",
                "Vật phẩm đang được trang bị, tháo ra trước khi vứt.");

        var session = _sessionManager.Get(_currentUser.SessionId);

        var change = await session.Handler.State.Waiters.SendAndWaitAsync<ItemChange>(
            itemId,
            () => session.GameServer.GetSender().SendPlayerThrowAwayItemPacket(itemId),
            TimeSpan.FromSeconds(TIME_WAIT_ASYNC),
            ItemSettle);

        if (change == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh vứt vật phẩm, có thể lệnh bị từ chối.");

        return true;
    }
}
