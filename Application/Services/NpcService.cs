using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Application.Mapper;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Store;
using BackendJX3D.Core.Utils;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

namespace BackendJX3D.Application.Services;

public class NpcService : INpcService
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;
    private readonly INpcMapper  _npcMapper;
    private readonly IItemMapper _itemMapper;

    public NpcService(ISessionManager sessionManager, ICurrentUser currentUser, INpcMapper  npcMapper, IItemMapper itemMapper)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
        _npcMapper = npcMapper;
        _itemMapper = itemMapper;
    }

    public async Task<List<NpcResponse>> GetListNpc()
    {

        var session = _sessionManager.Get(_currentUser.SessionId);

        var npcs = session.Handler.State.Npcs
            .GetAll()
            .Select(_npcMapper.FromNpcRequest)
            .ToList();

        return await Task.FromResult(npcs);
    }

    public async Task<NpcDialogResponse> OpenDialog(uint npcId)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        if (state.Npcs.Get(npcId) == null)
            throw new BaseException.NotFoundException(
                "npc_not_found",
                $"Không thấy NPC {npcId} quanh đây. Gọi GET /npc để lấy danh sách.");

        var sender = session.GameServer.GetSender();

        await WaitDialogOrShop(state, () => sender.SendNpcDialogPacket((int)npcId), npcId);

        return BuildDialogResponse(state, npcId);
    }

    public async Task<NpcDialogResponse> SelectDialogOption(int index)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var current = state.Dialog;

        if (current == null)
            throw new BaseException.ConflictException(
                "no_dialog_open",
                "Không có dialog nào đang mở. Gọi POST /npc/dialog trước.");

        if (index < 0)
            throw new BaseException.BadRequestException(
                "option_invalid",
                "index không hợp lệ.");

        var npcId = current.NpcId;
        var uiId = current.UiId;

        // SendClientCmdSelectUI: nSelectUi = UiId dialog
        var protocol = session.GameServer.Client.Protocol;

        await WaitDialogOrShop(state, () => protocol.SendClientCmdSelectUI(index, uiId), npcId);

        return BuildDialogResponse(state, npcId);
    }

    //PacketWaiter giữ chỗ cả 2 packet (mở shop, scritp), cái mô về trước thì lấy
    private static async Task WaitDialogOrShop(PlayerState state, Action send, uint npcId)
    {
        state.Dialog = null;
        state.Shop = null;

        long key = state.PlayerId;
        
        var shopTask = state.Waiters.SendAndWaitAsync<BUY_SELL_SYNC>(
            key, static () => { }, GameCommand.Timeout);

        var dialogTask = state.Waiters.SendAndWaitAsync<PLAYER_SCRIPTACTION_SYNC>(
            key, send, GameCommand.Timeout);

        await Task.WhenAny(shopTask, dialogTask);
        
        if (dialogTask.IsFaulted) await dialogTask;
        if (shopTask.IsFaulted) await shopTask;

        // Gói của GS không mang npcId, điền vào ở đây - lúc này recv thread đã dựng xong object
        if (state.Dialog != null) state.Dialog.NpcId = npcId;
        if (state.Shop != null) state.Shop.NpcId = npcId;
    }

    private NpcDialogResponse BuildDialogResponse(PlayerState state, uint npcId)
    {
        var response = _npcMapper.FromDialogRequest(state.Dialog, npcId);

        var shop = state.Shop;

        if (shop == null)
            return response;

        response.ShopOpened = true;
        response.Shop = _npcMapper.FromShopRequest(shop);

        return response;
    }

    public async Task<NpcShopResponse> GetShop()
    {
        var state = _sessionManager.Get(_currentUser.SessionId).Handler.State;

        return await Task.FromResult(_npcMapper.FromShopRequest(state.Shop));
    }

    // Nhịp dò lại túi đồ. 100ms cho ~30 lần dò trong 3 giây - đủ mịn mà không phải
    // chụp cả túi 60 lần như khi để 50ms.
    private static readonly TimeSpan BuyPollInterval = TimeSpan.FromMilliseconds(100);

    public async Task<ShopBuyResponse> BuyItem(int buyIdx, int count)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var state = session.Handler.State;

        var shop = state.Shop;

        if (shop == null)
            throw new BaseException.ConflictException(
                "no_shop_open",
                "Chưa có cửa hàng nào mở. Nói chuyện với NPC rồi chọn mục mở cửa hàng trước.");

        if (buyIdx < 0)
            throw new BaseException.BadRequestException(
                "buy_index_invalid",
                "buyIdx không hợp lệ.");

        if (count < 1)
            throw new BaseException.BadRequestException(
                "buy_count_invalid",
                "Số lượng mua phải từ 1 trở lên.");

        var response = new ShopBuyResponse
        {
            ShopIdx = shop.ShopIdx,
            BuyIdx = buyIdx,
            Count = count,
        };

        // Chụp túi TRƯỚC khi gửi. Đây là mốc xác nhận duy nhất dùng được: GS không trả
        // gói nào mang thứ đối chiếu với lệnh mua.
        var before = InventoryDiff.Snapshot(state.Items.GetAll());

        session.GameServer.GetSender().SendPlayerBuyItemPacket(shop.ShopIdx, buyIdx, count);

        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < GameCommand.Timeout)
        {
            await Task.Delay(BuyPollInterval);

            var gained = InventoryDiff.Gained(before, state.Items.GetAll());

            if (gained.Count == 0) continue;

            response.Success = true;
            response.WaitedMs = (long)(DateTime.UtcNow - start).TotalMilliseconds;
            response.Message = "Mua thành công.";

            response.Items = gained
                .Select(g => new ShopBuyItemResponse
                {
                    AddedCount = g.AddedCount,
                    IsNew = g.IsNew,
                    Item = _itemMapper.FromItemRequest(g.Item),
                })
                .ToList();

            return response;
        }

        response.WaitedMs = (long)(DateTime.UtcNow - start).TotalMilliseconds;
        response.Message =
            "Túi đồ không đổi sau khi gửi lệnh mua - game server đã bỏ qua lệnh. "
            + "Thường do sai buyIdx, không đủ tiền, không đủ cấp, hoặc túi đầy.";

        return response;
    }
}
