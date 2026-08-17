using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
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
            GameCommand.Timeout,
            GameCommand.ItemSettle);

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
            GameCommand.Timeout,
            GameCommand.ItemSettle);

        if (change == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh vứt vật phẩm, có thể lệnh bị từ chối.");

        return true;
    }



    public async Task<ChestResponse> GetChest()
    {
        var state = _sessionManager.Get(_currentUser.SessionId).Handler.State;

        return await Task.FromResult(new ChestResponse
        {
            Items = Snapshot(state, ITEM_POSITION.pos_exboxroom),
        });
    }

    public async Task<ItemMoveResponse> MoveItem(uint itemId, ITEM_POSITION? destPlace, byte destX, byte destY)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var item = FindItem(itemId);

        if (item.m_btPlace == (byte)ITEM_POSITION.pos_equip)
            throw new BaseException.BadRequestException(
                "item_equipped",
                "Vật phẩm đang mặc trên người, tháo trang bị trước khi chuyển.");

        // Để trống thì hiểu là đổi ô trong cùng kho hiện tại
        var target = (byte)(destPlace ?? (ITEM_POSITION)item.m_btPlace);

        if (target != (byte)ITEM_POSITION.pos_equiproom && target != (byte)ITEM_POSITION.pos_exboxroom)
            throw new BaseException.BadRequestException(
                "dest_place_unsupported",
                "Chỉ chuyển được giữa túi (pos_equiproom) và rương (pos_exboxroom).");

        // Không chặn "rương chưa mở": mình không theo dõi trạng thái mở/đóng nữa. Chuyển
        // vào rương mà GS chưa cho phép thì nó bỏ qua lệnh, API trả 504 với lý do đã ghi rõ.
        if (target == item.m_btPlace && destX == item.m_btX && destY == item.m_btY)
            throw new BaseException.BadRequestException(
                "same_slot",
                "Ô đích trùng ô hiện tại.");
        
        var moved = await state.Waiters.SendAndWaitAsync<ITEM_AUTO_MOVE_SYNC>(
            itemId,
            () => session.GameServer.GetSender().SendPlayerUseItemPacket(itemId, item.m_btPlace, target, destX, destY),
            GameCommand.Timeout,
            GameCommand.ItemSettle);

        if (moved == null)
            throw new BaseException.ErrorException(
                504,
                "Gameserver_timeout",
                "Game server không phản hồi lệnh chuyển vật phẩm, có thể ô đích đã có đồ "
                + "hoặc vật phẩm không được phép cất vào rương.");

        var after = state.Items.Get((int)itemId);

        return new ItemMoveResponse
        {
            ItemId = (int)itemId,

            // Lấy theo gói GS trả về, không theo cái mình gửi lên
            SrcPlace = moved.Value.m_btSrcPos,
            SrcX = moved.Value.m_btSrcX,
            SrcY = moved.Value.m_btSrcY,
            DestPlace = moved.Value.m_btDestPos,
            DestX = moved.Value.m_btDestX,
            DestY = moved.Value.m_btDestY,

            Item = after == null ? null : _itemMapper.FromItemRequest(after.Value),
            Inventory = Snapshot(state, ITEM_POSITION.pos_equiproom),
            Chest = Snapshot(state, ITEM_POSITION.pos_exboxroom),
        };
    }
}
